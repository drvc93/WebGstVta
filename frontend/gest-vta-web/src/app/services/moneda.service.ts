import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import type { Moneda } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class MonedaService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/monedas`;

  getAll(): Observable<Moneda[]> {
    return this.http.get<Moneda[]>(this.base);
  }

  getById(id: number): Observable<Moneda> {
    return this.http.get<Moneda>(`${this.base}/${id}`);
  }

  create(body: Omit<Moneda, 'id'>): Observable<Moneda> {
    return this.http.post<Moneda>(this.base, body);
  }

  update(id: number, body: Omit<Moneda, 'id'>): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
