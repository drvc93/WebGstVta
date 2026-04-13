export interface Compania {
  id: number;
  codigo: string;
  nombre: string;
  tipoDocumentoId: number;
  numeroDocumento: string;
  direccion?: string | null;
  paisId: number;
  ubigeoId?: number | null;
  correo?: string | null;
  activo: boolean;
  logoPath?: string | null;
  /** Color de marca (#RRGGBB) para tema de la aplicación. */
  colorPrimario?: string | null;
  telefono1?: string | null;
  telefono2?: string | null;
  ultUsuario?: string | null;
  ultMod?: string | null;
}

export interface TipoDocumento {
  id: number;
  codigo: string;
  nombre: string;
  activo: boolean;
}

export interface Pais {
  id: number;
  codigo: string;
  nombre: string;
  nombreEn?: string | null;
  iso3?: string | null;
  telefonoCodigo?: string | null;
  continente?: string | null;
  activo: boolean;
}

export interface Ubigeo {
  id: number;
  codigo: string;
  departamento: string;
  provincia: string;
  distrito: string;
  activo: boolean;
}

export interface Rol {
  id: number;
  codigo: string;
  nombre: string;
  activo: boolean;
}

export interface UsuarioListItem {
  id: number;
  username: string;
  nombreMostrar: string;
  activo: boolean;
  fechaCreacion: string;
  companiaId?: number | null;
  companiaIds: number[];
  rolCodigos: string[];
}

export interface UsuarioDetail {
  id: number;
  username: string;
  nombreMostrar: string;
  activo: boolean;
  fechaCreacion: string;
  companiaId?: number | null;
  companiaIds: number[];
  rolIds: number[];
}

export interface UsuarioSave {
  username: string;
  password?: string | null;
  nombreMostrar: string;
  activo: boolean;
  companiaIds: number[];
  rolIds: number[];
}

export interface MenuUsuarioOpcion {
  id: number;
  codigo: string;
  nombre: string;
  ruta?: string | null;
  icono?: string | null;
  parentId?: number | null;
  orden: number;
  puedeLeer: boolean;
  puedeEscribir: boolean;
  puedeModificar: boolean;
  puedeEliminar: boolean;
}

export interface MenuOpcion {
  id: number;
  codigo: string;
  nombre: string;
  ruta?: string | null;
  icono?: string | null;
  parentId?: number | null;
  orden: number;
  activo: boolean;
}

export interface MenuOpcionSave {
  codigo: string;
  nombre: string;
  ruta?: string | null;
  icono?: string | null;
  parentId?: number | null;
  orden: number;
  activo: boolean;
}

export interface RolMenuPermisoFila {
  menuOpcionId: number;
  codigo: string;
  nombre: string;
  ruta?: string | null;
  parentId?: number | null;
  orden: number;
  puedeLeer: boolean;
  puedeEscribir: boolean;
  puedeModificar: boolean;
  puedeEliminar: boolean;
}

export interface RolMenuPermisoGuardar {
  menuOpcionId: number;
  puedeLeer: boolean;
  puedeEscribir: boolean;
  puedeModificar: boolean;
  puedeEliminar: boolean;
}
