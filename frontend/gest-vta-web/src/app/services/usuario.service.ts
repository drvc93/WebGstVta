import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import type { UsuarioDetail, UsuarioListItem, UsuarioSave } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/usuarios`;

  getAll(): Observable<UsuarioListItem[]> {
    return this.http.get<UsuarioListItem[]>(this.base);
  }

  getById(id: number): Observable<UsuarioDetail> {
    return this.http.get<UsuarioDetail>(`${this.base}/${id}`);
  }

  create(body: UsuarioSave): Observable<UsuarioDetail> {
    return this.http.post<UsuarioDetail>(this.base, body);
  }

  update(id: number, body: UsuarioSave): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
