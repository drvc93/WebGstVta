export interface NavSection {
  id: string;
  label: string;
  /** Clase Bootstrap Icons (sin prefijo `bi `), ej. `bi-database`. */
  icon: string;
  items: { label: string; link: string[]; icon?: string; linkActiveExact?: boolean }[];
}
