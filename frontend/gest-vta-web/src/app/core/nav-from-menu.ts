import type { MenuUsuarioOpcion } from '../models/api.models';
import type { NavSection } from '../layout/nav-section.model';

function linkArrayFromRuta(ruta: string): string[] {
  const path = ruta.startsWith('/') ? ruta : `/${ruta}`;
  return [path];
}

function iconClass(icono?: string | null): string {
  if (!icono?.trim()) return 'bi-folder';
  return icono.startsWith('bi-') ? icono : `bi-${icono}`;
}

/** Construye secciones del lateral a partir del árbol devuelto por la API (solo entradas con lectura). */
export function buildNavFromMenu(rows: MenuUsuarioOpcion[]): NavSection[] {
  const byId = new Map(rows.map((r) => [r.id, r]));
  const isUnderRoot = (x: MenuUsuarioOpcion, rootId: number): boolean => {
    let p: number | null | undefined = x.parentId;
    while (p != null) {
      if (p === rootId) return true;
      p = byId.get(p)?.parentId ?? null;
    }
    return false;
  };
  const roots = rows.filter((r) => r.parentId == null).sort((a, b) => a.orden - b.orden);
  return roots
    .map((root) => {
      const items = rows
        .filter((x) => !!x.ruta?.trim() && x.puedeLeer && isUnderRoot(x, root.id))
        .sort((a, b) => a.orden - b.orden)
        .map((x) => ({
          label: x.nombre,
          link: linkArrayFromRuta(x.ruta!),
          icon: iconClass(x.icono),
          linkActiveExact: (x.ruta?.match(/\//g)?.length ?? 0) <= 2,
        }));
      return {
        id: root.codigo.toLowerCase().replace(/[^a-z0-9]+/g, '-'),
        label: root.nombre,
        icon: iconClass(root.icono),
        items,
      } satisfies NavSection;
    })
    .filter((s) => s.items.length > 0);
}
