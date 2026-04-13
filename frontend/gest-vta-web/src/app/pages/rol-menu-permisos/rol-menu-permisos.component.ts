import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { finalize } from 'rxjs';
import type { Rol } from '../../models/api.models';
import type { RolMenuPermisoFila, RolMenuPermisoGuardar } from '../../models/api.models';
import { CatalogosService } from '../../services/catalogos.service';
import { RolMenuPermisoService } from '../../services/rol-menu-permiso.service';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-rol-menu-permisos',
  templateUrl: './rol-menu-permisos.component.html',
  styleUrl: './rol-menu-permisos.component.scss',
})
export class RolMenuPermisosComponent implements OnInit {
  private readonly catalogos = inject(CatalogosService);
  private readonly api = inject(RolMenuPermisoService);
  private readonly toast = inject(ToastService);

  readonly roles = signal<Rol[]>([]);
  readonly rolId = signal<number | null>(null);
  readonly filas = signal<RolMenuPermisoFila[]>([]);
  readonly loadingRoles = signal(true);
  readonly loadingFilas = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly rolesActivos = computed(() =>
    [...this.roles()]
      .filter((r) => r.activo)
      .sort((a, b) => a.nombre.localeCompare(b.nombre, 'es')),
  );

  readonly filasOrdenadas = computed(() => {
    const list = [...this.filas()].sort((a, b) => {
      if (a.orden !== b.orden) return a.orden - b.orden;
      return a.menuOpcionId - b.menuOpcionId;
    });
    const byParent = new Map<number | null, RolMenuPermisoFila[]>();
    for (const f of list) {
      const k = f.parentId ?? null;
      const arr = byParent.get(k) ?? [];
      arr.push(f);
      byParent.set(k, arr);
    }
    const out: { f: RolMenuPermisoFila; depth: number }[] = [];
    const walk = (parentId: number | null, depth: number) => {
      const kids = byParent.get(parentId) ?? [];
      for (const f of kids) {
        out.push({ f, depth });
        walk(f.menuOpcionId, depth + 1);
      }
    };
    walk(null, 0);
    for (const f of list) {
      if (!out.some((x) => x.f.menuOpcionId === f.menuOpcionId)) out.push({ f, depth: 0 });
    }
    return out;
  });

  ngOnInit(): void {
    this.catalogos.roles().subscribe({
      next: (r) => {
        this.roles.set(r);
        this.loadingRoles.set(false);
        const first = r.find((x) => x.activo)?.id;
        if (first != null) this.seleccionarRol(first);
      },
      error: () => {
        this.error.set('No se pudieron cargar los roles.');
        this.loadingRoles.set(false);
      },
    });
  }

  seleccionarRol(id: number): void {
    this.rolId.set(id);
    this.cargarFilas(id);
  }

  cargarFilas(rolId: number): void {
    this.loadingFilas.set(true);
    this.error.set(null);
    this.api.getPorRol(rolId).subscribe({
      next: (rows) => {
        this.filas.set(rows);
        this.loadingFilas.set(false);
      },
      error: (e) => {
        const st = e?.status;
        this.error.set(
          st === 403 ? 'Solo ADMIN puede editar permisos de menú por rol.' : 'No se pudieron cargar los permisos.',
        );
        this.filas.set([]);
        this.loadingFilas.set(false);
      },
    });
  }

  patchFila(menuOpcionId: number, patch: Partial<Pick<RolMenuPermisoFila, 'puedeLeer' | 'puedeEscribir' | 'puedeModificar' | 'puedeEliminar'>>): void {
    this.filas.update((rows) =>
      rows.map((r) => (r.menuOpcionId === menuOpcionId ? { ...r, ...patch } : r)),
    );
  }

  onLeerChange(f: RolMenuPermisoFila, checked: boolean): void {
    if (!checked) {
      this.patchFila(f.menuOpcionId, {
        puedeLeer: false,
        puedeEscribir: false,
        puedeModificar: false,
        puedeEliminar: false,
      });
    } else {
      this.patchFila(f.menuOpcionId, { puedeLeer: true });
    }
  }

  guardar(): void {
    const rid = this.rolId();
    if (rid == null) return;
    const payload: RolMenuPermisoGuardar[] = this.filas().map((r) => ({
      menuOpcionId: r.menuOpcionId,
      puedeLeer: r.puedeLeer,
      puedeEscribir: r.puedeEscribir,
      puedeModificar: r.puedeModificar,
      puedeEliminar: r.puedeEliminar,
    }));
    this.saving.set(true);
    this.api
      .guardar(rid, payload)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.toast.success('Permisos guardados.');
          this.cargarFilas(rid);
        },
        error: () => this.toast.error('No se pudo guardar.'),
      });
  }
}
