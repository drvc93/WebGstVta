import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import type { Marca } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class MarcaService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/marcas`;

  getAll(): Observable<Marca[]> {
    return this.http.get<Marca[]>(this.base);
  }

  getById(id: number): Observable<Marca> {
    return this.http.get<Marca>(`${this.base}/${id}`);
  }

  create(body: Omit<Marca, 'id'>): Observable<Marca> {
    return this.http.post<Marca>(this.base, body);
  }

  update(id: number, body: Omit<Marca, 'id'>): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
