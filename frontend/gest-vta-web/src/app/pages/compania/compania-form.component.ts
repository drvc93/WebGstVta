import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import type { Compania, Pais, TipoDocumento } from '../../models/api.models';
import { CatalogosService } from '../../services/catalogos.service';
import { CompaniaService } from '../../services/compania.service';
import { ToastService } from '../../core/toast.service';
import { environment } from '../../../environments/environment';

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
  readonly logoPreviewUrl = signal<string | null>(null);
  private objectLogoUrl: string | null = null;

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
  readonly imageModalOpen = signal(false);
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
    this.destroyRef.onDestroy(() => this.revokeObjectLogoUrl());
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
    this.setLogoPreview(c.logoPath);
    this.loading.set(false);
  }

  logoPick(ev: Event): void {
    const input = ev.target as HTMLInputElement;
    const f = input.files?.[0];
    if (!f) return;
    const oldLogoPath = this.form.controls.logoPath.value?.trim() ?? '';
    this.revokeObjectLogoUrl();
    this.objectLogoUrl = URL.createObjectURL(f);
    this.logoPreviewUrl.set(this.objectLogoUrl);

    this.companias.uploadLogo(f).subscribe({
      next: (r) => {
        const webPath = r.path?.trim();
        if (!webPath) {
          this.toast.error('Respuesta de subida inválida.');
          return;
        }
        this.form.patchValue({ logoPath: webPath });
        this.revokeObjectLogoUrl();
        this.logoPreviewUrl.set(this.logoUrlForDisplay(webPath));
        input.value = '';
        if (oldLogoPath && oldLogoPath !== webPath) {
          this.companias.deleteLogo(oldLogoPath).subscribe({
            error: () => {
              // No bloquea la UX; solo evita archivos huérfanos cuando se puede.
            },
          });
        }
      },
      error: () => {
        this.toast.error('No se pudo subir el logo. Revise sesión y tamaño del archivo.');
        this.form.patchValue({ logoPath: '' });
        this.revokeObjectLogoUrl();
        this.logoPreviewUrl.set(null);
        input.value = '';
      },
    });
  }

  quitarLogo(): void {
    const oldLogoPath = this.form.controls.logoPath.value?.trim() ?? '';
    this.form.patchValue({ logoPath: '' });
    this.revokeObjectLogoUrl();
    this.logoPreviewUrl.set(null);
    if (!oldLogoPath) return;
    this.companias.deleteLogo(oldLogoPath).subscribe({
      next: () => this.toast.info('Logo eliminado.'),
      error: () => this.toast.error('Se quitó del formulario, pero no se pudo borrar el archivo en servidor.'),
    });
  }

  abrirVisorLogo(): void {
    if (!this.logoPreviewUrl()) return;
    this.imageModalOpen.set(true);
  }

  cerrarVisorLogo(): void {
    this.imageModalOpen.set(false);
  }

  private setLogoPreview(logoPath: string | null | undefined): void {
    this.revokeObjectLogoUrl();
    const raw = logoPath?.trim();
    if (!raw) {
      this.logoPreviewUrl.set(null);
      return;
    }
    this.logoPreviewUrl.set(this.logoUrlForDisplay(raw));
  }

  /** URL absoluta para <img> (API en otro puerto u origen). */
  private logoUrlForDisplay(storedPath: string): string | null {
    const raw = storedPath.trim();
    if (!raw) return null;
    if (raw.startsWith('http://') || raw.startsWith('https://') || raw.startsWith('data:image/') || raw.startsWith('blob:'))
      return raw;
    if (raw.startsWith('/files/'))
      return `${environment.apiUrl}${raw}`;
    if (raw.includes('/') || raw.includes('\\'))
      return raw.startsWith('/') ? `${environment.apiUrl}${raw}` : raw;
    return null;
  }

  private revokeObjectLogoUrl(): void {
    if (!this.objectLogoUrl) return;
    URL.revokeObjectURL(this.objectLogoUrl);
    this.objectLogoUrl = null;
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
