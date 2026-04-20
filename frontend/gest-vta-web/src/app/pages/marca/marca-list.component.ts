import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import type { Marca } from '../../models/api.models';
import { MarcaService } from '../../services/marca.service';

type SortKey = keyof Pick<Marca, 'codigo' | 'nombre' | 'activo' | 'ultUsuario' | 'ultMod'>;

@Component({
  selector: 'app-marca-list',
  imports: [DatePipe],
  templateUrl: './marca-list.component.html',
  styleUrl: './marca-list.component.scss',
})
export class MarcaListComponent implements OnInit {
  private readonly api = inject(MarcaService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly rows = signal<Marca[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly searchText = signal('');
  readonly sortKey = signal<SortKey>('codigo');
  readonly sortAsc = signal(true);
  readonly pageSize = signal(10);
  readonly pageIndex = signal(0);
  readonly pageSizeOptions = [5, 10, 25] as const;

  readonly filteredRows = computed(() => {
    const q = this.searchText().trim().toLowerCase();
    if (!q) return this.rows();
    return this.rows().filter((m) =>
      [m.codigo, m.nombre, m.ultUsuario ?? '', m.activo ? 'activo' : 'inactivo'].some((x) =>
        x.toString().toLowerCase().includes(q),
      ),
    );
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

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.sortedRows().length / this.pageSize())));

  readonly pageRows = computed(() => {
    const start = this.pageIndex() * this.pageSize();
    return this.sortedRows().slice(start, start + this.pageSize());
  });

  ngOnInit(): void {
    this.api.getAll().subscribe({
      next: (data) => {
        this.rows.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la lista de marcas.');
        this.loading.set(false);
      },
    });
  }

  setSearch(value: string): void {
    this.searchText.set(value);
    this.pageIndex.set(0);
  }

  clearSearch(): void {
    this.searchText.set('');
    this.pageIndex.set(0);
  }

  private sortValue(m: Marca, key: SortKey): string | number {
    switch (key) {
      case 'codigo':
      case 'nombre':
      case 'ultUsuario':
        return (m[key] ?? '').toString().toLowerCase();
      case 'ultMod':
        return m.ultMod ? new Date(m.ultMod).getTime() : 0;
      case 'activo':
        return m.activo ? 1 : 0;
      default:
        return '';
    }
  }

  toggleSort(key: SortKey): void {
    if (this.sortKey() === key) this.sortAsc.update((v) => !v);
    else {
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
