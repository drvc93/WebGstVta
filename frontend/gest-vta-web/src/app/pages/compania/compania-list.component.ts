import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import type { Compania } from '../../models/api.models';
import { CompaniaService } from '../../services/compania.service';

type SortKey = keyof Compania | 'ultMod';

@Component({
  selector: 'app-compania-list',
  imports: [DatePipe],
  templateUrl: './compania-list.component.html',
  styleUrl: './compania-list.component.scss',
})
export class CompaniaListComponent implements OnInit {
  private readonly api = inject(CompaniaService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly rows = signal<Compania[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly sortKey = signal<SortKey>('codigo');
  readonly sortAsc = signal(true);
  readonly pageSize = signal(5);
  readonly pageIndex = signal(0);

  /** Texto de búsqueda (coincidencia parcial, sin distinguir mayúsculas). */
  readonly searchText = signal('');

  readonly filteredRows = computed(() => {
    const q = this.searchText().trim().toLowerCase();
    if (!q) return this.rows();
    return this.rows().filter((c) => this.rowMatchesQuery(c, q));
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
    this.api.getAll().subscribe({
      next: (data) => {
        this.rows.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la lista. ¿Está la API y la base de datos disponibles?');
        this.loading.set(false);
      },
    });
  }

  private rowMatchesQuery(c: Compania, q: string): boolean {
    const haystacks: (string | null | undefined)[] = [
      c.codigo,
      c.nombre,
      c.numeroDocumento,
      c.telefono1,
      c.telefono2,
      c.ultUsuario,
      c.correo,
      c.direccion,
      c.activo ? 'activo' : 'inactivo',
    ];
    if (c.ultMod) {
      haystacks.push(c.ultMod);
      const d = new Date(c.ultMod);
      if (!Number.isNaN(d.getTime())) {
        haystacks.push(d.toLocaleDateString('es-PE'), d.toLocaleString('es-PE'));
      }
    }
    return haystacks.some((s) => (s ?? '').toString().toLowerCase().includes(q));
  }

  setSearch(value: string): void {
    this.searchText.set(value);
    this.pageIndex.set(0);
  }

  clearSearch(): void {
    this.searchText.set('');
    this.pageIndex.set(0);
  }

  private sortValue(c: Compania, key: SortKey): string | number {
    switch (key) {
      case 'codigo':
      case 'nombre':
      case 'numeroDocumento':
      case 'telefono1':
      case 'telefono2':
      case 'ultUsuario':
        return (c[key] ?? '').toString().toLowerCase();
      case 'ultMod':
        return c.ultMod ?? '';
      default:
        return '';
    }
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

  /** Estado visual del icono de ordenación en cabeceras. */
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
