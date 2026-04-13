import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { map, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { BrandThemeService } from './brand-theme.service';

const STORAGE_KEY = 'gestvta-session';

function readStoredSessionRaw(): string | null {
  if (typeof localStorage === 'undefined') return null;
  let raw = localStorage.getItem(STORAGE_KEY);
  if (!raw && typeof sessionStorage !== 'undefined') {
    raw = sessionStorage.getItem(STORAGE_KEY);
    if (raw) {
      localStorage.setItem(STORAGE_KEY, raw);
      sessionStorage.removeItem(STORAGE_KEY);
    }
  }
  return raw;
}

function writeStoredSession(json: string): void {
  if (typeof localStorage === 'undefined') return;
  localStorage.setItem(STORAGE_KEY, json);
  if (typeof sessionStorage !== 'undefined') {
    sessionStorage.removeItem(STORAGE_KEY);
  }
}

function removeStoredSession(): void {
  if (typeof localStorage !== 'undefined') {
    localStorage.removeItem(STORAGE_KEY);
  }
  if (typeof sessionStorage !== 'undefined') {
    sessionStorage.removeItem(STORAGE_KEY);
  }
}

export interface CompaniaLoginOption {
  id: number;
  codigo: string;
  nombre: string;
  colorPrimario?: string | null;
}

export type LoginOutcome =
  | { kind: 'success'; session: AuthSession }
  | { kind: 'choose_compania'; username: string; nombreMostrar: string; companias: CompaniaLoginOption[] };

export interface AuthSession {
  accessToken: string;
  /** ISO 8601 (UTC) de expiración del JWT. */
  expiresAt: string;
  username: string;
  nombreMostrar: string;
  roles: string[];
  companiaId?: number | null;
  companiaNombre?: string | null;
  colorPrimario: string;
}

interface LoginResponseDto {
  requiresCompaniaSelection: boolean;
  companiasDisponibles?: CompaniaLoginOption[] | null;
  accessToken?: string | null;
  expiresAt?: string | null;
  tokenType?: string | null;
  username?: string | null;
  nombreMostrar?: string | null;
  roles?: string[] | null;
  companiaId?: number | null;
  companiaNombre?: string | null;
  colorPrimario?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly brand = inject(BrandThemeService);

  /** Sesión actual (restaurada desde localStorage al arrancar). */
  readonly session = signal<AuthSession | null>(null);

  /** True si hay token y aún no ha pasado `expiresAt` (UTC). */
  isTokenExpiredNow(): boolean {
    const s = this.session();
    if (!s?.expiresAt) return true;
    return new Date(s.expiresAt).getTime() <= Date.now();
  }

  isLoggedIn(): boolean {
    const s = this.session();
    if (!s?.accessToken || !s.expiresAt) return false;
    if (this.isTokenExpiredNow()) {
      this.clearStoredSession();
      return false;
    }
    return true;
  }

  hydrateFromStorage(): void {
    const raw = readStoredSessionRaw();
    if (!raw) {
      this.session.set(null);
      this.brand.resetBrand();
      return;
    }
    try {
      const data = JSON.parse(raw) as AuthSession;
      if (!data.accessToken || !data.expiresAt || new Date(data.expiresAt).getTime() <= Date.now()) {
        this.clearStoredSession();
        return;
      }
      this.session.set(data);
      this.brand.applyFromHex(data.colorPrimario);
    } catch {
      this.clearStoredSession();
    }
  }

  /**
   * Si el usuario tiene varias compañías y no se envía `companiaId`, la API responde con `choose_compania` (sin token).
   * Repita la llamada con `companiaId` tras la elección en el modal.
   */
  login(username: string, password: string, companiaId?: number | null): Observable<LoginOutcome> {
    const body: { username: string; password: string; companiaId?: number } = { username, password };
    if (companiaId != null && companiaId > 0) {
      body.companiaId = companiaId;
    }

    return this.http.post<LoginResponseDto>(`${environment.apiUrl}/api/auth/login`, body).pipe(
      map((dto) => {
        if (dto.requiresCompaniaSelection) {
          return {
            kind: 'choose_compania' as const,
            username: dto.username ?? username,
            nombreMostrar: dto.nombreMostrar ?? '',
            companias: dto.companiasDisponibles ?? [],
          };
        }
        const session: AuthSession = {
          accessToken: dto.accessToken!,
          expiresAt: dto.expiresAt!,
          username: dto.username!,
          nombreMostrar: dto.nombreMostrar!,
          roles: dto.roles ?? [],
          companiaId: dto.companiaId,
          companiaNombre: dto.companiaNombre,
          colorPrimario: dto.colorPrimario ?? '#1a3a5c',
        };
        return { kind: 'success' as const, session };
      }),
      tap((outcome) => {
        if (outcome.kind === 'success') {
          writeStoredSession(JSON.stringify(outcome.session));
          this.session.set(outcome.session);
          this.brand.applyFromHex(outcome.session.colorPrimario);
        }
      }),
    );
  }

  /** Compañías del usuario autenticado (para cambiar de compañía sin cerrar sesión). */
  misCompanias(): Observable<CompaniaLoginOption[]> {
    return this.http.get<CompaniaLoginOption[]>(`${environment.apiUrl}/api/auth/mis-companias`);
  }

  /** Nuevo JWT con otra compañía; actualiza sesión y tema. */
  cambiarCompania(companiaId: number): Observable<void> {
    return this.http.post<LoginResponseDto>(`${environment.apiUrl}/api/auth/cambiar-compania`, { companiaId }).pipe(
      tap((dto) => {
        if (dto.requiresCompaniaSelection || !dto.accessToken || !dto.expiresAt) return;
        const s = this.session();
        if (!s) return;
        const next: AuthSession = {
          ...s,
          accessToken: dto.accessToken,
          expiresAt: dto.expiresAt,
          username: dto.username ?? s.username,
          nombreMostrar: dto.nombreMostrar ?? s.nombreMostrar,
          roles: dto.roles ?? s.roles,
          companiaId: dto.companiaId,
          companiaNombre: dto.companiaNombre,
          colorPrimario: dto.colorPrimario ?? '#1a3a5c',
        };
        writeStoredSession(JSON.stringify(next));
        this.session.set(next);
        this.brand.applyFromHex(next.colorPrimario);
      }),
      map(() => undefined),
    );
  }

  logout(): void {
    removeStoredSession();
    this.session.set(null);
    this.brand.resetBrand();
    void this.router.navigate(['/login']);
  }

  private clearStoredSession(): void {
    removeStoredSession();
    this.session.set(null);
    this.brand.resetBrand();
  }
}
