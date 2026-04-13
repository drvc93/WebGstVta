import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import type { RolMenuPermisoFila, RolMenuPermisoGuardar } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class RolMenuPermisoService {
  private readonly http = inject(HttpClient);

  getPorRol(rolId: number): Observable<RolMenuPermisoFila[]> {
    return this.http.get<RolMenuPermisoFila[]>(`${environment.apiUrl}/api/roles/${rolId}/permisos-menu`);
  }

  guardar(rolId: number, filas: RolMenuPermisoGuardar[]): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/api/roles/${rolId}/permisos-menu`, filas);
  }
}
