import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import type { Compania, Pais, TipoDocumento } from '../../models/api.models';
import { CatalogosService } from '../../services/catalogos.service';
import { CompaniaService } from '../../services/compania.service';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-compania-form',
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './compania-form.component.html',
  styleUrl: './compania-form.component.scss',
})
export class CompaniaFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly companias = inject(CompaniaService);
  private readonly catalogos = inject(CatalogosService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly tiposDoc = signal<TipoDocumento[]>([]);
  readonly paises = signal<Pais[]>([]);
  /** Texto para acotar el combo de países (246+ filas). */
  readonly paisBusqueda = signal('');
  /** Sincronizado con el formulario para mantener visible el país elegido al filtrar. */
  private readonly paisIdSeleccionado = signal<number | null>(null);
  readonly ubigeoOptions = signal<{ id: number; label: string }[]>([]);

  readonly paisesFiltrados = computed(() => {
    const q = this.paisBusqueda().trim().toLowerCase();
    const list = this.paises();
    const selId = this.paisIdSeleccionado();
    let base: Pais[];
    if (!q) base = list;
    else {
      base = list.filter(
        (p) =>
          p.nombre.toLowerCase().includes(q) ||
          p.codigo.toLowerCase().includes(q) ||
          (p.nombreEn ?? '').toLowerCase().includes(q) ||
          (p.iso3 ?? '').toLowerCase().includes(q) ||
          (p.continente ?? '').toLowerCase().includes(q),
      );
    }
    if (selId == null) return base;
    const picked = list.find((p) => p.id === selId);
    if (!picked || base.some((p) => p.id === selId)) return base;
    return [picked, ...base];
  });
  readonly loading = signal(true);
  readonly isEdit = signal(false);
  readonly confirmModalOpen = signal(false);
  readonly saving = signal(false);

  readonly tituloConfirmacion = computed(() =>
    this.isEdit() ? 'Confirmar actualización' : 'Confirmar registro',
  );

  readonly mensajeConfirmacion = computed(() =>
    this.isEdit()
      ? '¿Desea guardar los cambios de esta compañía?'
      : '¿Desea registrar esta nueva compañía?',
  );

  readonly form = this.fb.group({
    id: [0],
    codigo: ['', Validators.required],
    nombre: ['', Validators.required],
    tipoDocumentoId: [null as number | null, Validators.required],
    numeroDocumento: ['', Validators.required],
    direccion: [''],
    paisId: [null as number | null, Validators.required],
    ubigeoId: [null as number | null],
    correo: [''],
    activo: [true],
    logoPath: [''],
    colorPrimario: ['#1a3a5c'],
    telefono1: [''],
    telefono2: [''],
    ultUsuario: ['ADMIN'],
  });

  setPaisBusqueda(value: string): void {
    this.paisBusqueda.set(value);
  }

  private formValues() {
    return this.form.getRawValue() as {
      id: number;
      codigo: string;
      nombre: string;
      tipoDocumentoId: number;
      numeroDocumento: string;
      direccion: string;
      paisId: number;
      ubigeoId: number | null;
      correo: string;
      activo: boolean;
      logoPath: string;
      colorPrimario: string;
      telefono1: string;
      telefono2: string;
      ultUsuario: string;
    };
  }

  ngOnInit(): void {
    this.paisIdSeleccionado.set(this.form.get('paisId')?.value ?? null);
    this.form
      .get('paisId')
      ?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((v) => this.paisIdSeleccionado.set(v));

    this.catalogos.tiposDocumento().subscribe((t) => this.tiposDoc.set(t.filter((x) => x.activo)));
    this.catalogos.paises().subscribe((p) => this.paises.set(p.filter((x) => x.activo)));
    this.catalogos.ubigeos().subscribe((u) =>
      this.ubigeoOptions.set(
        u
          .filter((x) => x.activo)
          .map((x) => ({
            id: x.id,
            label: `${x.distrito} - ${x.provincia} - ${x.departamento}`,
          })),
      ),
    );

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      if (!Number.isNaN(id)) {
        this.isEdit.set(true);
        this.companias.getById(id).subscribe({
          next: (c) => this.patch(c),
          error: () => {
            this.toast.error('No se pudo cargar la compañía para edición.');
            this.loading.set(false);
          },
        });
        return;
      }
    }
    this.loading.set(false);
  }

  private patch(c: Compania): void {
    this.paisBusqueda.set('');
    this.form.patchValue({
      id: c.id,
      codigo: c.codigo,
      nombre: c.nombre,
      tipoDocumentoId: c.tipoDocumentoId,
      numeroDocumento: c.numeroDocumento,
      direccion: c.direccion ?? '',
      paisId: c.paisId,
      ubigeoId: c.ubigeoId ?? null,
      correo: c.correo ?? '',
      activo: c.activo,
      logoPath: c.logoPath ?? '',
      colorPrimario: c.colorPrimario?.trim() || '#1a3a5c',
      telefono1: c.telefono1 ?? '',
      telefono2: c.telefono2 ?? '',
      ultUsuario: c.ultUsuario ?? 'ADMIN',
    });
    this.loading.set(false);
  }

  logoPick(ev: Event): void {
    const input = ev.target as HTMLInputElement;
    const f = input.files?.[0];
    if (f) this.form.patchValue({ logoPath: f.name });
  }

  cancelar(): void {
    void this.router.navigate(['/maestros/compania']);
  }

  /** Paso 1: validar y mostrar modal de confirmación. */
  solicitarGuardado(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    if (v.tipoDocumentoId == null || v.paisId == null) return;
    this.confirmModalOpen.set(true);
  }

  cerrarConfirmacion(): void {
    if (this.saving()) return;
    this.confirmModalOpen.set(false);
  }

  /** Paso 2: tras confirmar en el modal, persistir en la API. */
  confirmarYGuardar(): void {
    if (this.form.invalid) {
      this.cerrarConfirmacion();
      return;
    }
    const v = this.formValues();
    if (v.tipoDocumentoId == null || v.paisId == null) return;

    const body: Compania = {
      id: v.id,
      codigo: v.codigo,
      nombre: v.nombre,
      tipoDocumentoId: v.tipoDocumentoId,
      numeroDocumento: v.numeroDocumento,
      direccion: v.direccion || null,
      paisId: v.paisId,
      ubigeoId: v.ubigeoId,
      correo: v.correo || null,
      activo: v.activo,
      logoPath: v.logoPath || null,
      colorPrimario: v.colorPrimario?.trim() ? v.colorPrimario.trim() : null,
      telefono1: v.telefono1 || null,
      telefono2: v.telefono2 || null,
      ultUsuario: v.ultUsuario || null,
      ultMod: null,
    };

    this.saving.set(true);

    const done = () => {
      this.confirmModalOpen.set(false);
      this.toast.success(this.isEdit() ? 'Compañía actualizada correctamente.' : 'Compañía registrada correctamente.');
      void this.router.navigate(['/maestros/compania']);
    };

    if (this.isEdit()) {
      this.companias
        .update(v.id, body)
        .pipe(finalize(() => this.saving.set(false)))
        .subscribe({
          next: done,
          error: () => this.toast.error('No se pudo actualizar la compañía. Intente nuevamente.'),
        });
    } else {
      body.id = 0;
      this.companias
        .create(body)
        .pipe(finalize(() => this.saving.set(false)))
        .subscribe({
          next: done,
          error: () => this.toast.error('No se pudo registrar la compañía. Intente nuevamente.'),
        });
    }
  }
}
