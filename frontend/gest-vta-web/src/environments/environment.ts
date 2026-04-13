/**
 * Valor por defecto en el IDE. En build NO se usa tal cual:
 * - `ng build` / producción → `environment.production.ts` (angular.json)
 * - `ng serve` / desarrollo → `environment.development.ts`
 */
export const environment = {
  production: true,
  apiUrl: 'http://38.242.219.173:82',
};
