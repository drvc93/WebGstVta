import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { finalize } from 'rxjs';
import { buildNavFromMenu } from '../core/nav-from-menu';
import type { CompaniaLoginOption } from '../core/auth.service';
import { AuthService } from '../core/auth.service';
import { MenuUsuarioService } from '../services/menu-usuario.service';
import { ThemeService } from '../core/theme.service';
import { ToastService } from '../core/toast.service';
import type { NavSection } from './nav-section.model';

export type { NavSection } from './nav-section.model';

const SIDEBAR_STORAGE_KEY = 'gestvta-sidebar-open';

function readSidebarOpenPref(): boolean {
  if (typeof localStorage === 'undefined') return true;
  return localStorage.getItem(SIDEBAR_STORAGE_KEY) !== '0';
}

const DEFAULT_NAV: NavSection[] = [
  {
    id: 'maestros',
    label: 'Maestros',
    icon: 'bi-database',
    items: [
      { label: 'Compañía', link: ['/maestros/compania'], icon: 'bi-building', linkActiveExact: true },
      { label: 'Usuarios', link: ['/maestros/usuarios'], icon: 'bi-person-badge', linkActiveExact: true },
      { label: 'Menú (opciones)', link: ['/maestros/menu-opciones'], icon: 'bi-list-nested', linkActiveExact: true },
      { label: 'Permisos por rol', link: ['/maestros/rol-permisos'], icon: 'bi-shield-lock', linkActiveExact: true },
      { label: 'Proceso', link: ['/maestros/proceso'], icon: 'bi-diagram-3' },
      { label: 'Adicionales', link: ['/maestros/adicionales'], icon: 'bi-plus-square' },
      { label: 'Rpta. Seguim.', link: ['/maestros/rpta-seguimiento'], icon: 'bi-chat-left-text' },
      { label: 'Forma Pago', link: ['/maestros/forma-pago'], icon: 'bi-credit-card-2-front' },
      { label: 'Moneda', link: ['/maestros/moneda'], icon: 'bi-currency-dollar' },
      { label: 'Tipo Documento', link: ['/maestros/tipo-documento'], icon: 'bi-file-earmark-text' },
      { label: 'Tipo Cambio', link: ['/maestros/tipo-cambio'], icon: 'bi-arrow-left-right' },
      { label: 'Segmento', link: ['/maestros/segmento'], icon: 'bi-diagram-2' },
    ],
  },
  {
    id: 'entidades',
    label: 'Entidades',
    icon: 'bi-people',
    items: [
      { label: 'Reg.Cliente', link: ['/entidades/cliente'], icon: 'bi-person-vcard' },
      { label: 'Agencia Transp.', link: ['/entidades/agencia-transporte'], icon: 'bi-truck' },
      { label: 'Grupo Cliente', link: ['/entidades/grupo-cliente'], icon: 'bi-people' },
      { label: 'Conductor', link: ['/entidades/conductor'], icon: 'bi-person-badge' },
      { label: 'Proveedor', link: ['/entidades/proveedor'], icon: 'bi-shop' },
    ],
  },
  {
    id: 'producto',
    label: 'Producto',
    icon: 'bi-box-seam',
    items: [
      { label: 'Items', link: ['/producto/items'], icon: 'bi-boxes' },
      { label: 'Unidad', link: ['/producto/unidad'], icon: 'bi-rulers' },
      { label: 'Familia', link: ['/producto/familia'], icon: 'bi-collection' },
      { label: 'Marca', link: ['/producto/marca'], icon: 'bi-award' },
      { label: 'Modelo', link: ['/producto/modelo'], icon: 'bi-grid-1x2' },
    ],
  },
];

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent implements OnInit {
  readonly theme = inject(ThemeService);
  readonly auth = inject(AuthService);
  readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly menuUsuario = inject(MenuUsuarioService);

  readonly companiasDisponibles = signal<CompaniaLoginOption[]>([]);
  readonly companiaPanelOpen = signal(false);
  readonly companiaSwitchLoading = signal(false);

  readonly mostrarCambioCompania = computed(() => this.companiasDisponibles().length > 1);

  readonly etiquetaCompaniaActual = computed(() => {
    const s = this.auth.session();
    const n = s?.companiaNombre?.trim();
    if (n) return n;
    if (s?.companiaId != null) return `Compañía #${s.companiaId}`;
    return 'Compañía';
  });

  /** Menú lateral desde API; null = usar respaldo estático. */
  readonly navFromApi = signal<NavSection[] | null>(null);

  readonly sectionsDisplay = computed(() => this.navFromApi() ?? DEFAULT_NAV);

  ngOnInit(): void {
    this.auth.misCompanias().subscribe({
      next: (list) => this.companiasDisponibles.set(list),
      error: () => {},
    });
    this.menuUsuario.miArbol().subscribe({
      next: (rows) => {
        const built = buildNavFromMenu(rows);
        if (built.length) this.navFromApi.set(built);
      },
      error: () => {
        /* sin tablas o sin permisos: menú estático */
      },
    });
  }

  toggleCompaniaPanel(): void {
    if (!this.mostrarCambioCompania()) return;
    this.companiaPanelOpen.update((v) => !v);
  }

  cerrarCompaniaPanel(): void {
    this.companiaPanelOpen.set(false);
  }

  companiaTieneSubtitulo(c: CompaniaLoginOption): boolean {
    const a = (c.codigo ?? '').trim().toLowerCase();
    const b = (c.nombre ?? '').trim().toLowerCase();
    return !!b && a !== b;
  }

  elegirOtraCompania(companiaId: number): void {
    const actual = this.auth.session()?.companiaId;
    if (companiaId === actual) {
      this.cerrarCompaniaPanel();
      return;
    }
    this.companiaSwitchLoading.set(true);
    this.auth
      .cambiarCompania(companiaId)
      .pipe(finalize(() => this.companiaSwitchLoading.set(false)))
      .subscribe({
        next: () => {
          this.toast.success('Compañía de trabajo actualizada.');
          this.cerrarCompaniaPanel();
        },
        error: () => this.toast.error('No se pudo cambiar de compañía.'),
      });
  }

  readonly displayName = computed(() => {
    const s = this.auth.session();
    if (!s) return 'Usuario';
    return s.nombreMostrar?.trim() || s.username;
  });

  readonly displayRole = computed(() => {
    const s = this.auth.session();
    if (!s?.roles?.length) return 'Sesión';
    return s.roles.join(', ');
  });

  readonly sidebarOpen = signal(readSidebarOpenPref());

  private readonly expandedSections = signal<ReadonlySet<string>>(
    new Set(['maestros', 'entidades', 'producto']),
  );
  readonly sidebarQuery = signal('');

  readonly filteredSections = computed(() => {
    const query = this.sidebarQuery().trim().toLowerCase();
    const base = this.sectionsDisplay();
    if (!query) return base;
    return base
      .map((section) => ({
        ...section,
        items: section.items.filter((item) => item.label.toLowerCase().includes(query)),
      }))
      .filter((section) => section.items.length > 0);
  });

  sectionOpen(id: string): boolean {
    return this.expandedSections().has(id);
  }

  toggleSection(id: string): void {
    this.expandedSections.update((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  clearSidebarQuery(): void {
    this.sidebarQuery.set('');
  }

  sectionIsActive(section: NavSection): boolean {
    return section.items.some((item) =>
      this.router.isActive(
        this.router.createUrlTree(item.link),
        {
          paths: item.linkActiveExact ? 'exact' : 'subset',
          queryParams: 'ignored',
          fragment: 'ignored',
          matrixParams: 'ignored',
        },
      ),
    );
  }

  toggleSidebar(): void {
    this.sidebarOpen.update((open) => {
      const next = !open;
      try {
        localStorage.setItem(SIDEBAR_STORAGE_KEY, next ? '1' : '0');
      } catch {
        /* ignore */
      }
      return next;
    });
  }
}
