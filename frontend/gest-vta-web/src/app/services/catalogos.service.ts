import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import type { Moneda, Pais, Rol, TipoDocumento, Ubigeo } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class CatalogosService {
  private readonly http = inject(HttpClient);
  private readonly root = environment.apiUrl;

  tiposDocumento(): Observable<TipoDocumento[]> {
    return this.http.get<TipoDocumento[]>(`${this.root}/api/tipos-documento`);
  }

  paises(): Observable<Pais[]> {
    return this.http.get<Pais[]>(`${this.root}/api/paises`);
  }

  ubigeos(): Observable<Ubigeo[]> {
    return this.http.get<Ubigeo[]>(`${this.root}/api/ubigeos`);
  }

  roles(): Observable<Rol[]> {
    return this.http.get<Rol[]>(`${this.root}/api/roles`);
  }

  monedas(): Observable<Moneda[]> {
    return this.http.get<Moneda[]>(`${this.root}/api/monedas`);
  }
}
