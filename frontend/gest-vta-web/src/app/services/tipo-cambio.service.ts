import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import type { TipoCambio } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class TipoCambioService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/tipos-cambio`;

  getAll(): Observable<TipoCambio[]> {
    return this.http.get<TipoCambio[]>(this.base);
  }

  getById(id: number): Observable<TipoCambio> {
    return this.http.get<TipoCambio>(`${this.base}/${id}`);
  }

  ensureHoy(): Observable<{ success: boolean; created: boolean; fecha: string; moneda: string }> {
    return this.http.post<{ success: boolean; created: boolean; fecha: string; moneda: string }>(`${this.base}/ensure-hoy`, {});
  }

  create(body: Omit<TipoCambio, 'id'>): Observable<TipoCambio> {
    return this.http.post<TipoCambio>(this.base, body);
  }

  update(id: number, body: Omit<TipoCambio, 'id'>): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
