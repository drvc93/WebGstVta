import type { MenuOpcion, MenuOpcionSave } from '../../models/api.models';

/** Borrador del formulario crear/editar menú. */
export type MenuOpcionDraft = MenuOpcionSave & { id?: number };

/** Nodo del árbol de menú bajo una sección (modal). */
export interface MenuOpcionTreeNode {
  m: MenuOpcion;
  /** Profundidad bajo la sección del modal (0 = primer nivel). */
  depth: number;
  children: MenuOpcionTreeNode[];
}
