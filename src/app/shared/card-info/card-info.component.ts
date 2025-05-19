import { Component, Input } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-card-info',
  templateUrl: './card-info.component.html',
  styleUrls: ['./card-info.component.scss']
})
export class CardInfoComponent {
  @Input() label!: string;
  @Input() value!: string | number;
}
