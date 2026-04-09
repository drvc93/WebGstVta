import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  template: `
    <div class="card gest-panel">
      <div class="card-header gest-card-header d-flex align-items-center gap-2">
        <i class="bi bi-speedometer2 text-primary" aria-hidden="true"></i>
        GestVta
      </div>
      <div class="card-body">
        <p class="gest-lead mb-0">
          Aplicación de maestros y ventas. Use el menú lateral para abrir cada formulario. La API .NET debe estar en
          ejecución y la base <strong>GestVta</strong> creada con los scripts SQL.
        </p>
      </div>
    </div>
  `,
})
export class HomeComponent {}
