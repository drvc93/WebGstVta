import { Component, input, output } from '@angular/core';
import type { MenuOpcion } from '../../../models/api.models';
import type { MenuOpcionDraft } from '../menu-opciones.types';
import { BootstrapIconPickerComponent } from '../../../shared/bootstrap-icon-picker.component';

@Component({
  selector: 'app-menu-opciones-form-modal',
  standalone: true,
  imports: [BootstrapIconPickerComponent],
  templateUrl: './menu-opciones-form-modal.component.html',
  styleUrl: './menu-opciones-form-modal.component.scss',
})
export class MenuOpcionesFormModalComponent {
  readonly editingId = input.required<number | null>();
  readonly draft = input.required<MenuOpcionDraft>();
  readonly padreFijoAlCrear = input.required<number | null>();
  readonly padreFijoNombre = input('');
  readonly parentOptions = input.required<MenuOpcion[]>();
  readonly saving = input(false);

  readonly closed = output<void>();
  readonly draftPatch = output<Partial<MenuOpcionDraft>>();
  readonly saveRequested = output<void>();
}
