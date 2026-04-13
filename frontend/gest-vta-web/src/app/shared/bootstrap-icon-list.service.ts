import { Injectable, signal } from '@angular/core';

interface IconifyBiJson {
  icons: Record<string, unknown>;
}

@Injectable({ providedIn: 'root' })
export class BootstrapIconListService {
  private readonly names = signal<string[]>([]);
  private readonly loading = signal(false);
  private loadPromise: Promise<void> | null = null;

  readonly iconNames = this.names.asReadonly();
  readonly isLoading = this.loading.asReadonly();

  /** Carga perezosa de todos los slugs Bootstrap Icons (vía @iconify-json/bi). */
  ensureLoaded(): Promise<void> {
    if (this.names().length > 0) return Promise.resolve();
    if (this.loadPromise) return this.loadPromise;

    this.loading.set(true);
    this.loadPromise = import('@iconify-json/bi/icons.json')
      .then((mod) => {
        const m = mod as { default?: IconifyBiJson } & IconifyBiJson;
        const data = m.default ?? m;
        const keys = Object.keys(data.icons).sort((a, b) => a.localeCompare(b, 'es'));
        this.names.set(keys);
      })
      .finally(() => this.loading.set(false));

    return this.loadPromise;
  }
}
