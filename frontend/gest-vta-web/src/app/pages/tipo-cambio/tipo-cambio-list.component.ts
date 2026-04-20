import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import type { Moneda, TipoCambio } from '../../models/api.models';
import { CatalogosService } from '../../services/catalogos.service';
import { TipoCambioService } from '../../services/tipo-cambio.service';
import { switchMap } from 'rxjs';

type SortKey = 'fecha' | 'valorCompra' | 'valorVenta' | 'activo' | 'monedaId';

@Component({
  selector: 'app-tipo-cambio-list',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './tipo-cambio-list.component.html',
  styleUrl: './tipo-cambio-list.component.scss',
})
export class TipoCambioListComponent implements OnInit {
  private readonly api = inject(TipoCambioService);
  private readonly catalogos = inject(CatalogosService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly rows = signal<TipoCambio[]>([]);
  readonly monedas = signal<Moneda[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly searchText = signal('');
  readonly sortKey = signal<SortKey>('fecha');
  readonly sortAsc = signal(false);
  readonly pageSize = signal(10);
  readonly pageIndex = signal(0);
  readonly pageSizeOptions = [5, 10, 25] as const;

  readonly monedaNombre = computed(() => {
    const map = new Map<number, string>();
    for (const m of this.monedas()) map.set(m.id, `${m.codigo} - ${m.nombre}`);
    return map;
  });

  readonly filteredRows = computed(() => {
    const q = this.searchText().trim().toLowerCase();
    if (!q) return this.rows();
    return this.rows().filter((r) => {
      const moneda = this.monedaNombre().get(r.monedaId) ?? `#${r.monedaId}`;
      return [moneda, r.fecha, String(r.valorCompra), String(r.valorVenta), r.activo ? 'activo' : 'inactivo']
        .some((x) => (x ?? '').toString().toLowerCase().includes(q));
    });
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
    this.catalogos.monedas().subscribe({
      next: (m) => this.monedas.set(m.filter((x) => x.activo)),
      error: () => void 0,
    });

    this.api
      .ensureHoy()
      .pipe(switchMap(() => this.api.getAll()))
      .subscribe({
        next: (data) => {
          this.rows.set(data);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('No se pudo verificar/obtener tipo de cambio de hoy.');
          this.loading.set(false);
        },
      });
  }

  monedaLabel(monedaId: number): string {
    return this.monedaNombre().get(monedaId) ?? `#${monedaId}`;
  }

  setSearch(value: string): void {
    this.searchText.set(value);
    this.pageIndex.set(0);
  }

  clearSearch(): void {
    this.searchText.set('');
    this.pageIndex.set(0);
  }

  private sortValue(r: TipoCambio, key: SortKey): string | number {
    switch (key) {
      case 'monedaId':
        return this.monedaLabel(r.monedaId).toLowerCase();
      case 'fecha':
        return r.fecha ?? '';
      case 'valorCompra':
      case 'valorVenta':
        return Number(r[key]);
      case 'activo':
        return r.activo ? 1 : 0;
      default:
        return '';
    }
  }

  toggleSort(key: SortKey): void {
    if (this.sortKey() === key) this.sortAsc.update((v) => !v);
    else {
      this.sortKey.set(key);
      this.sortAsc.set(key === 'fecha' ? false : true);
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

