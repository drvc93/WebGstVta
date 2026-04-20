import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import type { TipoDocumento } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class TipoDocumentoService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/tipos-documento`;

  getAll(): Observable<TipoDocumento[]> {
    return this.http.get<TipoDocumento[]>(this.base);
  }

  getById(id: number): Observable<TipoDocumento> {
    return this.http.get<TipoDocumento>(`${this.base}/${id}`);
  }

  create(body: Omit<TipoDocumento, 'id'>): Observable<TipoDocumento> {
    return this.http.post<TipoDocumento>(this.base, body);
  }

  update(id: number, body: Omit<TipoDocumento, 'id'>): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
