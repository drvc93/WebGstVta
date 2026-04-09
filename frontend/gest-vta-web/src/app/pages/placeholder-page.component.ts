import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';

@Component({
  selector: 'app-placeholder-page',
  template: `
    <div class="card gest-panel">
      <div class="card-header gest-card-header d-flex align-items-center gap-2">
        <i class="bi bi-file-earmark-code text-primary" aria-hidden="true"></i>
        {{ title() }}
      </div>
      <div class="card-body">
        <p class="gest-lead mb-0">
          Pantalla base para CRUD. Endpoint REST:
          <code>{{ apiHint() }}</code>
        </p>
      </div>
    </div>
  `,
})
export class PlaceholderPageComponent {
  private readonly route = inject(ActivatedRoute);

  private readonly dataTitle = toSignal(
    this.route.data.pipe(map((d) => (d['title'] as string) ?? 'Módulo')),
    { initialValue: 'Módulo' },
  );

  readonly title = computed(() => this.dataTitle());

  private readonly api = toSignal(this.route.data.pipe(map((d) => (d['api'] as string) ?? '')), {
    initialValue: '',
  });

  readonly apiHint = computed(() => this.api() || '(definir en rutas data.api)');
}
