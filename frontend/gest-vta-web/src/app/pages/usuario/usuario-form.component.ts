import { Component, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { UsuarioFormStore } from './usuario-form.store';

@Component({
  selector: 'app-usuario-form',
  imports: [ReactiveFormsModule],
  templateUrl: './usuario-form.component.html',
  styleUrl: './usuario-form.component.scss',
  providers: [UsuarioFormStore],
})
export class UsuarioFormComponent implements OnInit {
  readonly vm = inject(UsuarioFormStore);

  ngOnInit(): void {
    this.vm.init();
  }
}
