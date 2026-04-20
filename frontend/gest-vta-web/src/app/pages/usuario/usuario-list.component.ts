import { DatePipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { UsuarioListStore } from './usuario-list.store';

@Component({
  selector: 'app-usuario-list',
  imports: [DatePipe],
  templateUrl: './usuario-list.component.html',
  styleUrl: './usuario-list.component.scss',
  providers: [UsuarioListStore],
})
export class UsuarioListComponent implements OnInit {
  readonly vm = inject(UsuarioListStore);

  ngOnInit(): void {
    this.vm.load();
  }
}
