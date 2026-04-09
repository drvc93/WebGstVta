import { Injectable, signal } from '@angular/core';

export type ToastKind = 'success' | 'danger' | 'warning' | 'info';

export interface ToastMessage {
  id: number;
  kind: ToastKind;
  text: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly toasts = signal<ToastMessage[]>([]);
  private seq = 0;

  success(text: string, timeoutMs = 3200): void {
    this.push('success', text, timeoutMs);
  }

  error(text: string, timeoutMs = 4200): void {
    this.push('danger', text, timeoutMs);
  }

  info(text: string, timeoutMs = 3000): void {
    this.push('info', text, timeoutMs);
  }

  remove(id: number): void {
    this.toasts.update((prev) => prev.filter((t) => t.id !== id));
  }

  private push(kind: ToastKind, text: string, timeoutMs: number): void {
    const id = ++this.seq;
    this.toasts.update((prev) => [...prev, { id, kind, text }]);
    window.setTimeout(() => this.remove(id), timeoutMs);
  }
}
