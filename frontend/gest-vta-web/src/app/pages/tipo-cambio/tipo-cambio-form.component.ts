import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { readApiError } from '../../core/http-error.util';
import { ToastService } from '../../core/toast.service';
import type { Moneda } from '../../models/api.models';
import { CatalogosService } from '../../services/catalogos.service';
import { TipoCambioService } from '../../services/tipo-cambio.service';

@Component({
  selector: 'app-tipo-cambio-form',
  imports: [ReactiveFormsModule],
  templateUrl: './tipo-cambio-form.component.html',
  styleUrl: './tipo-cambio-form.component.scss',
})
export class TipoCambioFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(TipoCambioService);
  private readonly catalogos = inject(CatalogosService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly isEdit = signal(false);
  readonly monedas = signal<Moneda[]>([]);

  readonly form = this.fb.nonNullable.group({
    monedaId: [0, [Validators.required, Validators.min(1)]],
    fecha: ['', [Validators.required]],
    valorCompra: [0, [Validators.required, Validators.min(0)]],
    valorVenta: [0, [Validators.required, Validators.min(0)]],
    activo: [true],
  });

  ngOnInit(): void {
    this.catalogos.monedas().subscribe({
      next: (rows) => this.monedas.set(rows.filter((m) => m.activo)),
      error: () => this.toast.error('No se pudo cargar monedas.'),
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    const esNuevo = idParam === 'nuevo';
    this.isEdit.set(!esNuevo && !!idParam);

    if (esNuevo || !idParam) {
      this.loading.set(false);
      return;
    }

    const id = +idParam;
    this.api
      .getById(id)
      .pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (m) => {
          this.form.patchValue({
            monedaId: m.monedaId,
            fecha: (m.fecha ?? '').slice(0, 10),
            valorCompra: Number(m.valorCompra),
            valorVenta: Number(m.valorVenta),
            activo: m.activo,
          });
        },
        error: () => {
          this.toast.error('No se pudo cargar el tipo de cambio.');
          void this.router.navigate(['/maestros/tipo-cambio']);
        },
      });
  }

  volver(): void {
    void this.router.navigate(['/maestros/tipo-cambio']);
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toast.error('Revise los campos obligatorios.');
      return;
    }

    const v = this.form.getRawValue();
    const body = {
      monedaId: Number(v.monedaId),
      fecha: v.fecha,
      valorCompra: Number(v.valorCompra),
      valorVenta: Number(v.valorVenta),
      activo: v.activo,
      ultUsuario: 'ADMIN',
      ultMod: null,
    };

    this.saving.set(true);
    if (this.isEdit()) {
      const id = +this.route.snapshot.paramMap.get('id')!;
      this.api
        .update(id, body)
        .pipe(finalize(() => this.saving.set(false)), takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.toast.success('Tipo de cambio actualizado.');
            void this.router.navigate(['/maestros/tipo-cambio']);
          },
          error: (err) => this.toast.error(readApiError(err, 'No se pudo guardar.')),
        });
    } else {
      this.api
        .create(body)
        .pipe(finalize(() => this.saving.set(false)), takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.toast.success('Tipo de cambio registrado.');
            void this.router.navigate(['/maestros/tipo-cambio']);
          },
          error: (err) => this.toast.error(readApiError(err, 'No se pudo registrar.')),
        });
    }
  }
}

