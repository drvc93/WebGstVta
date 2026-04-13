import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import type { Compania, UsuarioListItem } from '../../models/api.models';
import { CompaniaService } from '../../services/compania.service';
import { UsuarioService } from '../../services/usuario.service';

type SortKey = 'username' | 'nombreMostrar' | 'activo' | 'fechaCreacion';

@Component({
  selector: 'app-usuario-list',
  imports: [DatePipe],
  templateUrl: './usuario-list.component.html',
  styleUrl: './usuario-list.component.scss',
})
export class UsuarioListComponent implements OnInit {
  private readonly api = inject(UsuarioService);
  private readonly companiasApi = inject(CompaniaService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly rows = signal<UsuarioListItem[]>([]);
  readonly companias = signal<Compania[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly sortKey = signal<SortKey>('username');
  readonly sortAsc = signal(true);
  readonly pageSize = signal(10);
  readonly pageIndex = signal(0);
  readonly searchText = signal('');

  readonly companiaNombre = computed(() => {
    const map = new Map<number, string>();
    for (const c of this.companias()) {
      map.set(c.id, `${c.codigo} — ${c.nombre}`);
    }
    return map;
  });

  readonly filteredRows = computed(() => {
    const q = this.searchText().trim().toLowerCase();
    if (!q) return this.rows();
    return this.rows().filter((u) => this.rowMatches(u, q));
  });

  readonly sortedRows = computed(() => {
    const key = this.sortKey();
    const asc = this.sortAsc();
    const data = [...this.filteredRows()];
    const mul = asc ? 1 : -1;
    data.sort((a, b) => {
      const va = this.sortValue(a, key);
      const vb = this.sortValue(b, key);
      if (va < vb) return -1 * mul;
      if (va > vb) return 1 * mul;
      return 0;
    });
    return data;
  });

  readonly totalPages = computed(() => {
    const n = this.sortedRows().length;
    const ps = this.pageSize();
    return Math.max(1, Math.ceil(n / ps));
  });

  readonly pageRows = computed(() => {
    const start = this.pageIndex() * this.pageSize();
    return this.sortedRows().slice(start, start + this.pageSize());
  });

  readonly pageSizeOptions = [5, 10, 25] as const;

  ngOnInit(): void {
    this.companiasApi.getAll().subscribe({
      next: (c) => this.companias.set(c),
      error: () => {
        /* lista de usuarios puede cargar sin nombres de compañía */
      },
    });
    this.api.getAll().subscribe({
      next: (data) => {
        this.rows.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la lista de usuarios.');
        this.loading.set(false);
      },
    });
  }

  companiasLabel(u: UsuarioListItem): string {
    const map = this.companiaNombre();
    return (u.companiaIds ?? [])
      .map((id) => map.get(id) ?? `#${id}`)
      .join(', ');
  }

  rolesLabel(u: UsuarioListItem): string {
    return (u.rolCodigos ?? []).join(', ');
  }

  private rowMatches(u: UsuarioListItem, q: string): boolean {
    const parts = [
      u.username,
      u.nombreMostrar,
      u.activo ? 'activo' : 'inactivo',
      ...((u.rolCodigos ?? []) as string[]),
      this.companiasLabel(u),
    ];
    return parts.some((s) => (s ?? '').toString().toLowerCase().includes(q));
  }

  private sortValue(u: UsuarioListItem, key: SortKey): string | number {
    switch (key) {
      case 'username':
      case 'nombreMostrar':
        return (u[key] ?? '').toString().toLowerCase();
      case 'activo':
        return u.activo ? 1 : 0;
      case 'fechaCreacion':
        return u.fechaCreacion ?? '';
      default:
        return '';
    }
  }

  setSearch(value: string): void {
    this.searchText.set(value);
    this.pageIndex.set(0);
  }

  clearSearch(): void {
    this.searchText.set('');
    this.pageIndex.set(0);
  }

  toggleSort(key: SortKey): void {
    if (this.sortKey() === key) {
      this.sortAsc.update((v) => !v);
    } else {
      this.sortKey.set(key);
      this.sortAsc.set(true);
    }
    this.pageIndex.set(0);
  }

  sortIconState(key: SortKey): 'asc' | 'desc' | 'none' {
    if (this.sortKey() !== key) return 'none';
    return this.sortAsc() ? 'asc' : 'desc';
  }

  setPageSize(n: number): void {
    this.pageSize.set(n);
    this.pageIndex.set(0);
  }

  prevPage(): void {
    this.pageIndex.update((i) => Math.max(0, i - 1));
  }

  nextPage(): void {
    this.pageIndex.update((i) => Math.min(this.totalPages() - 1, i + 1));
  }

  nuevo(): void {
    void this.router.navigate(['nuevo'], { relativeTo: this.route });
  }

  editar(id: number): void {
    void this.router.navigate([String(id)], { relativeTo: this.route });
  }
}
