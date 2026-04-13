import { Component, computed, inject, OnInit, signal, viewChild } from '@angular/core';
import { finalize } from 'rxjs';
import type { MenuOpcion, MenuOpcionSave } from '../../models/api.models';
import { MenuOpcionService } from '../../services/menu-opcion.service';
import { ToastService } from '../../core/toast.service';
import type { MenuOpcionDraft, MenuOpcionTreeNode } from './menu-opciones.types';
import { MenuOpcionesFormModalComponent } from './menu-opciones-form-modal/menu-opciones-form-modal.component';
import { MenuOpcionesTreeModalComponent } from './menu-opciones-tree-modal/menu-opciones-tree-modal.component';

@Component({
  selector: 'app-menu-opciones-list',
  imports: [MenuOpcionesTreeModalComponent, MenuOpcionesFormModalComponent],
  templateUrl: './menu-opciones-list.component.html',
  styleUrl: './menu-opciones-list.component.scss',
})
export class MenuOpcionesListComponent implements OnInit {
  private readonly api = inject(MenuOpcionService);
  private readonly toast = inject(ToastService);

  readonly treeModal = viewChild(MenuOpcionesTreeModalComponent);

  readonly rows = signal<MenuOpcion[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly saving = signal(false);
  readonly panelOpen = signal(false);
  readonly editingId = signal<number | null>(null);
  /** Al crear desde el modal: `parentId` fijo (no se muestra el combo). */
  readonly padreFijoAlCrear = signal<number | null>(null);

  readonly draft = signal<MenuOpcionDraft>(this.emptyDraft());

  readonly padreFijoLabel = computed(() => {
    const id = this.padreFijoAlCrear();
    return id != null ? this.nombreOpcion(id) : '';
  });

  /** Solo secciones raíz (`ParentId` nulo), ordenadas. */
  readonly raicesOrdenadas = computed(() =>
    [...this.rows()]
      .filter((m) => m.parentId == null)
      .sort((a, b) => {
        if (a.orden !== b.orden) return a.orden - b.orden;
        return a.id - b.id;
      }),
  );

  readonly modalPadre = signal<MenuOpcion | null>(null);

  /** Subárbol jerárquico bajo la sección del modal (solo hijos directos del padre en la raíz del array). */
  readonly arbolModal = computed((): MenuOpcionTreeNode[] => {
    const pad = this.modalPadre();
    if (!pad) return [];
    const byParent = this.buildChildrenByParentId();
    const build = (parentId: number, depth: number): MenuOpcionTreeNode[] =>
      (byParent.get(parentId) ?? []).map((m) => ({
        m,
        depth,
        children: build(m.id, depth + 1),
      }));
    return build(pad.id, 0);
  });

  readonly parentOptions = computed(() => {
    const editId = this.editingId();
    const rows = this.rows();
    const blocked = editId != null ? this.descendantIds(editId, rows) : new Set<number>();
    return rows
      .filter((r) => editId == null || r.id !== editId)
      .filter((r) => !blocked.has(r.id))
      .sort((a, b) => a.nombre.localeCompare(b.nombre, 'es'));
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getAll().subscribe({
      next: (data) => {
        this.rows.set(data);
        const mp = this.modalPadre();
        if (mp) {
          const upd = data.find((r) => r.id === mp.id);
          if (upd) this.modalPadre.set(upd);
          else this.modalPadre.set(null);
        }
        this.loading.set(false);
      },
      error: (e) => {
        const st = e?.status;
        this.error.set(
          st === 403
            ? 'Solo usuarios con rol ADMIN pueden administrar el menú.'
            : 'No se pudo cargar el catálogo de opciones de menú.',
        );
        this.loading.set(false);
      },
    });
  }

  contarSubarbol(padreId: number): number {
    return this.subarbolDesde(padreId).length;
  }

  nombreOpcion(id: number): string {
    return this.rows().find((r) => r.id === id)?.nombre ?? `#${id}`;
  }

  abrirModalHijos(p: MenuOpcion): void {
    this.modalPadre.set(p);
    setTimeout(() => this.treeModal()?.expandAll(), 0);
  }

  cerrarModalHijos(): void {
    this.cerrarPanel();
    this.modalPadre.set(null);
  }

  nuevo(): void {
    this.cerrarModalHijos();
    this.editingId.set(null);
    this.padreFijoAlCrear.set(null);
    this.draft.set(this.emptyDraft());
    this.panelOpen.set(true);
  }

  nuevoHijo(padreId: number): void {
    this.editingId.set(null);
    this.padreFijoAlCrear.set(padreId);
    this.draft.set({ ...this.emptyDraft(), parentId: padreId, orden: 0 });
    this.panelOpen.set(true);
  }

  editar(m: MenuOpcion): void {
    this.editingId.set(m.id);
    this.padreFijoAlCrear.set(null);
    this.draft.set({
      id: m.id,
      codigo: m.codigo,
      nombre: m.nombre,
      ruta: m.ruta ?? '',
      icono: m.icono ?? '',
      parentId: m.parentId ?? null,
      orden: m.orden,
      activo: m.activo,
    });
    this.panelOpen.set(true);
  }

  cerrarPanel(): void {
    this.panelOpen.set(false);
    this.editingId.set(null);
    this.padreFijoAlCrear.set(null);
  }

  patchDraft(p: Partial<MenuOpcionDraft>): void {
    this.draft.update((d) => ({ ...d, ...p }));
  }

  guardar(): void {
    const d = this.draft();
    const codigo = d.codigo.trim();
    const nombre = d.nombre.trim();
    if (!codigo || !nombre) {
      this.toast.error('Código y nombre son obligatorios.');
      return;
    }
    const fijo = this.padreFijoAlCrear();
    const parentId = fijo ?? d.parentId ?? null;
    const body: MenuOpcionSave = {
      codigo,
      nombre,
      ruta: d.ruta?.trim() || null,
      icono: d.icono?.trim() || null,
      parentId,
      orden: Number.isFinite(d.orden) ? d.orden : 0,
      activo: d.activo,
    };
    this.saving.set(true);
    const id = this.editingId();
    const done = finalize(() => this.saving.set(false));
    if (id == null) {
      this.api.create(body).pipe(done).subscribe({
        next: () => {
          this.toast.success('Opción creada.');
          this.cerrarPanel();
          this.reload();
        },
        error: (e: unknown) => {
          const msg =
            e && typeof e === 'object' && 'error' in e && typeof (e as { error?: unknown }).error === 'string'
              ? (e as { error: string }).error
              : 'No se pudo guardar.';
          this.toast.error(msg);
        },
      });
    } else {
      this.api.update(id, body).pipe(done).subscribe({
        next: () => {
          this.toast.success('Opción actualizada.');
          this.cerrarPanel();
          this.reload();
        },
        error: (e: unknown) => {
          const msg =
            e && typeof e === 'object' && 'error' in e && typeof (e as { error?: unknown }).error === 'string'
              ? (e as { error: string }).error
              : 'No se pudo guardar.';
          this.toast.error(msg);
        },
      });
    }
  }

  eliminar(m: MenuOpcion): void {
    if (!confirm(`¿Eliminar la opción «${m.nombre}» (${m.codigo})?`)) return;
    this.api.delete(m.id).subscribe({
      next: () => {
        this.toast.success('Opción eliminada.');
        const modal = this.modalPadre();
        if (modal?.id === m.id) this.cerrarModalHijos();
        this.reload();
      },
      error: (e: unknown) => {
        const msg =
          e && typeof e === 'object' && 'error' in e && typeof (e as { error?: unknown }).error === 'string'
            ? (e as { error: string }).error
            : 'No se pudo eliminar.';
        this.toast.error(msg);
      },
    });
  }

  private emptyDraft(): MenuOpcionDraft {
    return {
      codigo: '',
      nombre: '',
      ruta: '',
      icono: '',
      parentId: null,
      orden: 0,
      activo: true,
    };
  }

  private buildChildrenByParentId(): Map<number | null, MenuOpcion[]> {
    const list = [...this.rows()].sort((a, b) => {
      if (a.orden !== b.orden) return a.orden - b.orden;
      return a.id - b.id;
    });
    const byParent = new Map<number | null, MenuOpcion[]>();
    for (const m of list) {
      const k = m.parentId ?? null;
      const arr = byParent.get(k) ?? [];
      arr.push(m);
      byParent.set(k, arr);
    }
    return byParent;
  }

  private subarbolDesde(raizId: number): { m: MenuOpcion; depth: number }[] {
    const byParent = this.buildChildrenByParentId();
    const out: { m: MenuOpcion; depth: number }[] = [];
    const walk = (parentId: number, depth: number) => {
      const kids = byParent.get(parentId) ?? [];
      for (const m of kids) {
        out.push({ m, depth });
        walk(m.id, depth + 1);
      }
    };
    walk(raizId, 0);
    return out;
  }

  private descendantIds(rootId: number, rows: MenuOpcion[]): Set<number> {
    const byParent = new Map<number | null, number[]>();
    for (const m of rows) {
      const k = m.parentId ?? null;
      const arr = byParent.get(k) ?? [];
      arr.push(m.id);
      byParent.set(k, arr);
    }
    const out = new Set<number>();
    const stack = [...(byParent.get(rootId) ?? [])];
    while (stack.length) {
      const id = stack.pop()!;
      if (out.has(id)) continue;
      out.add(id);
      for (const c of byParent.get(id) ?? []) stack.push(c);
    }
    return out;
  }
}
