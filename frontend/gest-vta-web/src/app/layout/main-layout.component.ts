import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth.service';
import { ThemeService } from '../core/theme.service';
import { ToastService } from '../core/toast.service';

const SIDEBAR_STORAGE_KEY = 'gestvta-sidebar-open';

function readSidebarOpenPref(): boolean {
  if (typeof localStorage === 'undefined') return true;
  return localStorage.getItem(SIDEBAR_STORAGE_KEY) !== '0';
}

export interface NavSection {
  id: string;
  label: string;
  /** Clase Bootstrap Icons (sin prefijo `bi `), ej. `bi-database`. */
  icon: string;
  items: { label: string; link: string[]; icon?: string; linkActiveExact?: boolean }[];
}

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  readonly theme = inject(ThemeService);
  readonly auth = inject(AuthService);
  readonly toast = inject(ToastService);
  private readonly router = inject(Router);

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

  /** Panel lateral visible (preferencia en localStorage). */
  readonly sidebarOpen = signal(readSidebarOpenPref());

  /** Secciones abiertas en el lateral (sin depender del JS de Bootstrap). */
  private readonly expandedSections = signal<ReadonlySet<string>>(
    new Set(['maestros', 'entidades', 'producto']),
  );
  readonly sidebarQuery = signal('');

  readonly sections: NavSection[] = [
    {
      id: 'maestros',
      label: 'Maestros',
      icon: 'bi-database',
      items: [
        { label: 'Compañía', link: ['/maestros/compania'], icon: 'bi-building', linkActiveExact: true },
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

  readonly filteredSections = computed(() => {
    const query = this.sidebarQuery().trim().toLowerCase();
    if (!query) return this.sections;
    return this.sections
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
