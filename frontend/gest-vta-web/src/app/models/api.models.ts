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
