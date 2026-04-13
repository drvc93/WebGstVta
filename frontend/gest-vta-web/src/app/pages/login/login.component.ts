import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import type { CompaniaLoginOption } from '../../core/auth.service';
import { AuthService } from '../../core/auth.service';

/** Tras validar usuario/contraseña, si hay varias compañías es obligatorio este paso antes de entrar a la app. */
type LoginPhase = 'credentials' | 'elegir_compania';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly errorMsg = signal<string | null>(null);

  readonly phase = signal<LoginPhase>('credentials');
  readonly companiasPick = signal<CompaniaLoginOption[]>([]);
  readonly pickNombreMostrar = signal('');
  private pendingUsername = '';
  private pendingPassword = '';

  readonly form = this.fb.nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { username, password } = this.form.getRawValue();
    this.errorMsg.set(null);
    this.loading.set(true);
    this.auth
      .login(username.trim(), password)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (out) => {
          if (out.kind === 'choose_compania') {
            this.pendingUsername = out.username;
            this.pendingPassword = password;
            this.pickNombreMostrar.set(out.nombreMostrar);
            this.companiasPick.set(out.companias);
            this.phase.set('elegir_compania');
            return;
          }
          void this.router.navigateByUrl('/');
        },
        error: () => this.errorMsg.set('Usuario o contraseña incorrectos.'),
      });
  }

  elegirCompania(companiaId: number): void {
    this.errorMsg.set(null);
    this.loading.set(true);
    this.auth
      .login(this.pendingUsername, this.pendingPassword, companiaId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (out) => {
          if (out.kind === 'success') {
            this.limpiarPendiente();
            void this.router.navigateByUrl('/');
            return;
          }
          this.errorMsg.set('No se pudo completar el acceso. Vuelva a intentarlo.');
        },
        error: () => this.errorMsg.set('Usuario o contraseña incorrectos.'),
      });
  }

  /** Vuelve al formulario de acceso (aún no hay sesión). */
  volverACredenciales(): void {
    if (this.loading()) return;
    this.limpiarPendiente();
    this.phase.set('credentials');
    this.errorMsg.set(null);
  }

  private limpiarPendiente(): void {
    this.companiasPick.set([]);
    this.pickNombreMostrar.set('');
    this.pendingUsername = '';
    this.pendingPassword = '';
  }

  /** Evita repetir la misma línea cuando código y razón social coinciden. */
  companiaLineaSecundaria(c: CompaniaLoginOption): string | null {
    const a = (c.codigo ?? '').trim().toLowerCase();
    const n = (c.nombre ?? '').trim();
    const b = n.toLowerCase();
    if (!n || a === b) return null;
    return n;
  }

  colorCompania(c: CompaniaLoginOption): string {
    const x = c.colorPrimario?.trim();
    return x && x.startsWith('#') ? x : '#1a3a5c';
  }
}
