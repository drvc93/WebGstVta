import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import type { Compania } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class CompaniaService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/companias`;

  getAll(): Observable<Compania[]> {
    return this.http.get<Compania[]>(this.base);
  }

  getById(id: number): Observable<Compania> {
    return this.http.get<Compania>(`${this.base}/${id}`);
  }

  create(body: Compania): Observable<Compania> {
    return this.http.post<Compania>(this.base, body);
  }

  update(id: number, body: Compania): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  /** Sube logo a FileStorage del servidor; devuelve la ruta web (p. ej. /files/companias/....png) para LogoPath. */
  uploadLogo(file: File): Observable<{ path: string }> {
    const fd = new FormData();
    fd.append('file', file, file.name);
    return this.http.post<{ path: string }>(`${this.base}/logo`, fd);
  }

  /** Elimina físicamente un logo en FileStorage usando su ruta web (/files/companias/...). */
  deleteLogo(path: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/logo`, { params: { path } });
  }
}
