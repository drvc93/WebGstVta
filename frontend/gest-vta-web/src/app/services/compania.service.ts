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
}
