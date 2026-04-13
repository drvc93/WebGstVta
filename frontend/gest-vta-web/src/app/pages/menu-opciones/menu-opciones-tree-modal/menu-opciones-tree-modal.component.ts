import { Component, input, output, viewChild, type TrackByFunction } from '@angular/core';
import { CdkTree, CdkTreeModule } from '@angular/cdk/tree';
import type { MenuOpcion } from '../../../models/api.models';
import type { MenuOpcionTreeNode } from '../menu-opciones.types';

@Component({
  selector: 'app-menu-opciones-tree-modal',
  standalone: true,
  imports: [CdkTreeModule],
  templateUrl: './menu-opciones-tree-modal.component.html',
  styleUrl: './menu-opciones-tree-modal.component.scss',
})
export class MenuOpcionesTreeModalComponent {
  readonly menuCdkTree = viewChild<CdkTree<MenuOpcionTreeNode, number>>('menuCdkTree');

  readonly seccion = input.required<MenuOpcion>();
  readonly nodes = input.required<MenuOpcionTreeNode[]>();
  readonly totalEnArbol = input.required<number>();

  readonly closed = output<void>();
  readonly agregarEnSeccion = output<void>();
  readonly agregarHijoDe = output<number>();
  readonly editar = output<MenuOpcion>();
  readonly eliminar = output<MenuOpcion>();

  readonly treeChildrenAccessor = (node: MenuOpcionTreeNode) => node.children;
  readonly treeTrackBy: TrackByFunction<MenuOpcionTreeNode> = (_i, node) => node.m.id;
  readonly treeExpansionKey = (node: MenuOpcionTreeNode) => node.m.id;

  expandAll(): void {
    this.menuCdkTree()?.expandAll();
  }

  iconClassBi(icono?: string | null): string {
    const s = icono?.trim();
    if (!s) return 'bi-layout-text-window-reverse';
    return s.startsWith('bi-') ? s : `bi-${s}`;
  }

  etiquetaNivel(depth: number): string {
    if (depth <= 0) return 'Opción principal';
    if (depth === 1) return 'Sub-opción';
    return `Sub-opción (nivel ${depth + 1})`;
  }
}
