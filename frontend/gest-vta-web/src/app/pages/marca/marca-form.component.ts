import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { readApiError } from '../../core/http-error.util';
import { ToastService } from '../../core/toast.service';
import { MarcaService } from '../../services/marca.service';

@Component({
  selector: 'app-marca-form',
  imports: [ReactiveFormsModule],
  templateUrl: './marca-form.component.html',
  styleUrl: './marca-form.component.scss',
})
export class MarcaFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(MarcaService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly isEdit = signal(false);

  readonly form = this.fb.nonNullable.group({
    codigo: ['', [Validators.required, Validators.maxLength(20)]],
    nombre: ['', [Validators.required, Validators.maxLength(120)]],
    activo: [true],
  });

  ngOnInit(): void {
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
            codigo: m.codigo,
            nombre: m.nombre,
            activo: m.activo,
          });
        },
        error: () => {
          this.toast.error('No se pudo cargar la marca.');
          void this.router.navigate(['/producto/marca']);
        },
      });
  }

  volver(): void {
    void this.router.navigate(['/producto/marca']);
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toast.error('Revise los campos obligatorios.');
      return;
    }

    const v = this.form.getRawValue();
    const body = {
      codigo: v.codigo.trim().toUpperCase(),
      nombre: v.nombre.trim(),
      activo: v.activo,
    };

    this.saving.set(true);
    if (this.isEdit()) {
      const id = +this.route.snapshot.paramMap.get('id')!;
      this.api
        .update(id, body)
        .pipe(finalize(() => this.saving.set(false)), takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.toast.success('Marca actualizada.');
            void this.router.navigate(['/producto/marca']);
          },
          error: (err) => this.toast.error(readApiError(err, 'No se pudo guardar.')),
        });
    } else {
      this.api
        .create(body)
        .pipe(finalize(() => this.saving.set(false)), takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.toast.success('Marca registrada.');
            void this.router.navigate(['/producto/marca']);
          },
          error: (err) => this.toast.error(readApiError(err, 'No se pudo registrar.')),
        });
    }
  }
}
