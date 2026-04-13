import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs';
import type { Compania, Rol } from '../../models/api.models';
import { ToastService } from '../../core/toast.service';
import { CatalogosService } from '../../services/catalogos.service';
import { CompaniaService } from '../../services/compania.service';
import { UsuarioService } from '../../services/usuario.service';

function readApiError(err: unknown, fallback: string): string {
  if (!err || typeof err !== 'object') return fallback;
  const e = err as { error?: unknown; message?: string };
  const body = e.error;
  if (typeof body === 'string' && body.trim()) return body;
  if (body && typeof body === 'object') {
    const d = (body as { detail?: unknown; title?: unknown }).detail ?? (body as { title?: unknown }).title;
    if (typeof d === 'string' && d.trim()) return d;
  }
  if (typeof e.message === 'string' && e.message.trim()) return e.message;
  return fallback;
}

@Component({
  selector: 'app-usuario-form',
  imports: [ReactiveFormsModule],
  templateUrl: './usuario-form.component.html',
  styleUrl: './usuario-form.component.scss',
})
export class UsuarioFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly usuarios = inject(UsuarioService);
  private readonly companiasApi = inject(CompaniaService);
  private readonly catalogos = inject(CatalogosService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly companias = signal<Compania[]>([]);
  readonly roles = signal<Rol[]>([]);
  /** Orden de compañías: el primero es el predeterminado (Usuario.CompaniaId en API). */
  readonly companiaIdsOrden = signal<number[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly isEdit = signal(false);

  readonly form = this.fb.group({
    username: ['', [Validators.required, Validators.maxLength(80)]],
    password: [''],
    nombreMostrar: ['', [Validators.required, Validators.maxLength(200)]],
    activo: [true],
  });

  readonly companiasParaAgregar = computed(() => {
    const set = new Set(this.companiaIdsOrden());
    return this.companias()
      .filter((c) => c.activo && !set.has(c.id))
      .sort((a, b) => a.codigo.localeCompare(b.codigo, 'es'));
  });

  readonly rolesActivos = computed(() => this.roles().filter((r) => r.activo).sort((a, b) => a.nombre.localeCompare(b.nombre, 'es')));

  private rolSeleccionados = new Set<number>();

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const isNuevo = idParam === 'nuevo';
    this.isEdit.set(!isNuevo && !!idParam);

    forkJoin({
      companias: this.companiasApi.getAll(),
      roles: this.catalogos.roles(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ companias, roles }) => {
          this.companias.set(companias);
          this.roles.set(roles);
          if (isNuevo || !idParam) {
            this.loading.set(false);
            this.form.controls.password.setValidators([Validators.required, Validators.minLength(4)]);
            this.form.controls.password.updateValueAndValidity();
            return;
          }
          const id = +idParam;
          this.form.controls.password.clearValidators();
          this.form.controls.password.updateValueAndValidity();
          this.usuarios
            .getById(id)
            .pipe(
              finalize(() => this.loading.set(false)),
              takeUntilDestroyed(this.destroyRef),
            )
            .subscribe({
              next: (u) => {
                this.form.patchValue({
                  username: u.username,
                  nombreMostrar: u.nombreMostrar,
                  activo: u.activo,
                });
                this.companiaIdsOrden.set([...(u.companiaIds ?? [])]);
                this.rolSeleccionados = new Set(u.rolIds ?? []);
              },
              error: () => {
                this.toast.error('No se pudo cargar el usuario.');
                void this.router.navigate(['/maestros/usuarios']);
              },
            });
        },
        error: () => {
          this.loading.set(false);
          this.toast.error('No se pudieron cargar catálogos (compañías / roles).');
        },
      });
  }

  isRolChecked(id: number): boolean {
    return this.rolSeleccionados.has(id);
  }

  toggleRol(id: number, checked: boolean): void {
    if (checked) this.rolSeleccionados.add(id);
    else this.rolSeleccionados.delete(id);
  }

  onAgregarCompania(select: HTMLSelectElement): void {
    const v = select.value;
    select.value = '';
    if (!v) return;
    const id = +v;
    if (!Number.isFinite(id)) return;
    this.companiaIdsOrden.update((list) => [...list, id]);
  }

  quitarCompania(id: number): void {
    this.companiaIdsOrden.update((list) => list.filter((x) => x !== id));
  }

  moverCompania(id: number, delta: number): void {
    this.companiaIdsOrden.update((list) => {
      const i = list.indexOf(id);
      if (i < 0) return list;
      const j = i + delta;
      if (j < 0 || j >= list.length) return list;
      const next = [...list];
      [next[i], next[j]] = [next[j]!, next[i]!];
      return next;
    });
  }

  companiaLabel(id: number): string {
    const c = this.companias().find((x) => x.id === id);
    return c ? `${c.codigo} — ${c.nombre}` : `#${id}`;
  }

  volver(): void {
    void this.router.navigate(['/maestros/usuarios']);
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toast.error('Revise los campos obligatorios.');
      return;
    }
    const raw = this.form.getRawValue();
    const companiaIds = this.companiaIdsOrden();
    const rolIds = [...this.rolSeleccionados].sort((a, b) => a - b);
    if (rolIds.length === 0) {
      this.toast.error('Debe asignar al menos un rol.');
      return;
    }
    const password = (raw.password ?? '').trim();
    if (!this.isEdit() && password.length < 4) {
      this.toast.error('La contraseña es obligatoria (mínimo 4 caracteres).');
      return;
    }

    const body = {
      username: raw.username!.trim(),
      password: password.length ? password : null,
      nombreMostrar: raw.nombreMostrar!.trim(),
      activo: !!raw.activo,
      companiaIds,
      rolIds,
    };

    this.saving.set(true);
    if (this.isEdit()) {
      const id = +this.route.snapshot.paramMap.get('id')!;
      this.usuarios
        .update(id, body)
        .pipe(finalize(() => this.saving.set(false)), takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.toast.success('Usuario actualizado.');
            void this.router.navigate(['/maestros/usuarios']);
          },
          error: (err) => this.toast.error(readApiError(err, 'No se pudo guardar.')),
        });
    } else {
      this.usuarios
        .create({ ...body, password: password })
        .pipe(finalize(() => this.saving.set(false)), takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.toast.success('Usuario registrado.');
            void this.router.navigate(['/maestros/usuarios']);
          },
          error: (err) => this.toast.error(readApiError(err, 'No se pudo registrar.')),
        });
    }
  }
}
