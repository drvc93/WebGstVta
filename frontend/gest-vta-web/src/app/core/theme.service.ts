import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'gestvta-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  /** true = oscuro (clase app-dark + Bootstrap data-bs-theme=dark) */
  readonly dark = signal(false);

  initFromStorage(): void {
    const stored = localStorage.getItem(STORAGE_KEY);
    const dark =
      stored === 'dark' ? true : stored === 'light' ? false : window.matchMedia('(prefers-color-scheme: dark)').matches;
    this.dark.set(dark);
    this.applyDom(dark);
  }

  setDark(dark: boolean): void {
    this.dark.set(dark);
    this.applyDom(dark);
    localStorage.setItem(STORAGE_KEY, dark ? 'dark' : 'light');
  }

  toggle(): void {
    this.setDark(!this.dark());
  }

  private applyDom(dark: boolean): void {
    const html = document.documentElement;
    const body = document.body;
    html.classList.toggle('app-dark', dark);
    body.classList.toggle('app-dark', dark);
    html.setAttribute('data-bs-theme', dark ? 'dark' : 'light');
  }
}
