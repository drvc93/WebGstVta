import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { BrandThemeService } from './brand-theme.service';

const STORAGE_KEY = 'gestvta-session';

export interface AuthSession {
  username: string;
  nombreMostrar: string;
  roles: string[];
  companiaId?: number | null;
  companiaNombre?: string | null;
  colorPrimario: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly brand = inject(BrandThemeService);

  /** Sesión actual (restaurada desde sessionStorage al arrancar). */
  readonly session = signal<AuthSession | null>(null);

  isLoggedIn(): boolean {
    return this.session() !== null;
  }

  hydrateFromStorage(): void {
    const raw = typeof sessionStorage !== 'undefined' ? sessionStorage.getItem(STORAGE_KEY) : null;
    if (!raw) {
      this.session.set(null);
      this.brand.resetBrand();
      return;
    }
    try {
      const data = JSON.parse(raw) as AuthSession;
      this.session.set(data);
      this.brand.applyFromHex(data.colorPrimario);
    } catch {
      this.session.set(null);
      this.brand.resetBrand();
    }
  }

  login(username: string, password: string): Observable<AuthSession> {
    return this.http
      .post<AuthSession>(`${environment.apiUrl}/api/auth/login`, { username, password })
      .pipe(
        tap((data) => {
          sessionStorage.setItem(STORAGE_KEY, JSON.stringify(data));
          this.session.set(data);
          this.brand.applyFromHex(data.colorPrimario);
        }),
      );
  }

  logout(): void {
    sessionStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
    this.brand.resetBrand();
    void this.router.navigate(['/login']);
  }
}
