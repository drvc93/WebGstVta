import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import type { MenuUsuarioOpcion } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class MenuUsuarioService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/menu-usuario`;

  miArbol(): Observable<MenuUsuarioOpcion[]> {
    return this.http.get<MenuUsuarioOpcion[]>(`${this.base}/mi-arbol`);
  }
}
