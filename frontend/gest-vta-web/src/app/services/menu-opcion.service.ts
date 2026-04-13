import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import type { MenuOpcion, MenuOpcionSave } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class MenuOpcionService {
  private readonly http = inject(HttpClient);
  private readonly root = `${environment.apiUrl}/api/menu-opciones`;

  getAll(): Observable<MenuOpcion[]> {
    return this.http.get<MenuOpcion[]>(this.root);
  }

  getById(id: number): Observable<MenuOpcion> {
    return this.http.get<MenuOpcion>(`${this.root}/${id}`);
  }

  create(body: MenuOpcionSave): Observable<MenuOpcion> {
    return this.http.post<MenuOpcion>(this.root, body);
  }

  update(id: number, body: MenuOpcionSave): Observable<void> {
    return this.http.put<void>(`${this.root}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.root}/${id}`);
  }
}
