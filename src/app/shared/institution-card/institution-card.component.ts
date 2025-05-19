import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-institution-card',
  templateUrl: './institution-card.component.html',
  styleUrls: ['./institution-card.component.scss']
})
export class InstitutionCardComponent {
  @Input() name!: string;
  @Input() cause!: string;

  @Output() donate = new EventEmitter<string>(); // envia o nome da instituição (pode trocar depois por id, objeto etc.)
}
