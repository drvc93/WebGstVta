import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { loginGuard } from './core/login.guard';
import { MainLayoutComponent } from './layout/main-layout.component';
import { CompaniaFormComponent } from './pages/compania/compania-form.component';
import { CompaniaListComponent } from './pages/compania/compania-list.component';
import { UsuarioFormComponent } from './pages/usuario/usuario-form.component';
import { UsuarioListComponent } from './pages/usuario/usuario-list.component';
import { HomeComponent } from './pages/home.component';
import { LoginComponent } from './pages/login/login.component';
import { PlaceholderPageComponent } from './pages/placeholder-page.component';
import { MenuOpcionesListComponent } from './pages/menu-opciones/menu-opciones-list.component';
import { RolMenuPermisosComponent } from './pages/rol-menu-permisos/rol-menu-permisos.component';

const ph = (title: string, api: string) => ({
  component: PlaceholderPageComponent,
  data: { title, api },
});

export const routes: Routes = [
  { path: 'login', component: LoginComponent, canActivate: [loginGuard] },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', component: HomeComponent },
      { path: 'maestros/compania/nuevo', component: CompaniaFormComponent },
      { path: 'maestros/compania/:id', component: CompaniaFormComponent },
      { path: 'maestros/compania', component: CompaniaListComponent },
      { path: 'maestros/usuarios/nuevo', component: UsuarioFormComponent },
      { path: 'maestros/usuarios/:id', component: UsuarioFormComponent },
      { path: 'maestros/usuarios', component: UsuarioListComponent },
      { path: 'maestros/menu-opciones', component: MenuOpcionesListComponent },
      { path: 'maestros/rol-permisos', component: RolMenuPermisosComponent },
      { path: 'maestros/proceso', ...ph('Proceso', '/api/procesos') },
      { path: 'maestros/adicionales', ...ph('Adicionales', '/api/adicionales') },
      { path: 'maestros/rpta-seguimiento', ...ph('Respuesta seguimiento', '/api/rptas-seguimiento') },
      { path: 'maestros/forma-pago', ...ph('Forma pago', '/api/formas-pago') },
      { path: 'maestros/moneda', ...ph('Moneda', '/api/monedas') },
      { path: 'maestros/tipo-documento', ...ph('Tipo documento', '/api/tipos-documento') },
      { path: 'maestros/tipo-cambio', ...ph('Tipo cambio', '/api/tipos-cambio') },
      { path: 'maestros/segmento', ...ph('Segmento', '/api/segmentos') },
      { path: 'entidades/cliente', ...ph('Registro cliente', '/api/clientes') },
      { path: 'entidades/agencia-transporte', ...ph('Agencia transporte', '/api/agencias-transporte') },
      { path: 'entidades/grupo-cliente', ...ph('Grupo cliente', '/api/grupos-cliente') },
      { path: 'entidades/conductor', ...ph('Conductor', '/api/conductores') },
      { path: 'entidades/proveedor', ...ph('Proveedor', '/api/proveedores') },
      { path: 'producto/items', ...ph('Items', '/api/items') },
      { path: 'producto/unidad', ...ph('Unidad', '/api/unidades') },
      { path: 'producto/familia', ...ph('Familia', '/api/familias') },
      { path: 'producto/marca', ...ph('Marca', '/api/marcas') },
      { path: 'producto/modelo', ...ph('Modelo', '/api/modelos') },
    ],
  },
];
