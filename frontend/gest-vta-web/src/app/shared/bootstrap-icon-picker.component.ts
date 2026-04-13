import { Component, computed, inject, input, output, signal } from '@angular/core';
import { BootstrapIconListService } from './bootstrap-icon-list.service';

/** Slug almacenado sin prefijo `bi-` (compatible con `iconClassBi` / menú lateral). */
@Component({
  selector: 'app-bootstrap-icon-picker',
  standalone: true,
  templateUrl: './bootstrap-icon-picker.component.html',
  styleUrl: './bootstrap-icon-picker.component.scss',
})
export class BootstrapIconPickerComponent {
  /** Expuesto a la plantilla (lista Bootstrap Icons / Iconify). */
  readonly iconSvc = inject(BootstrapIconListService);

  /** Valor actual (p. ej. `house` o `bi-house`). */
  readonly icono = input<string>('');

  readonly iconoChange = output<string>();

  readonly panelOpen = signal(false);
  readonly search = signal('');

  readonly filtered = computed(() => {
    const all = this.iconSvc.iconNames();
    const q = this.search().trim().toLowerCase();
    if (!q) return all.slice(0, 120);
    const hit = all.filter((n) => n.includes(q));
    return hit.slice(0, 200);
  });

  togglePanel(): void {
    const next = !this.panelOpen();
    this.panelOpen.set(next);
    if (next) {
      this.search.set(this.normalizedSlug(this.icono()));
      void this.iconSvc.ensureLoaded();
    }
  }

  closePanel(): void {
    this.panelOpen.set(false);
  }

  pick(slug: string): void {
    this.iconoChange.emit(slug);
    this.closePanel();
  }

  clearIcon(): void {
    this.iconoChange.emit('');
    this.closePanel();
  }

  biClass(slug: string): string {
    return `bi-${slug}`;
  }

  normalizedSlug(raw: string): string {
    const s = raw?.trim() ?? '';
    if (!s) return '';
    return s.startsWith('bi-') ? s.slice(3) : s;
  }
}
