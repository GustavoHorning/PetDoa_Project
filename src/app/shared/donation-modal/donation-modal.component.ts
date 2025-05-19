import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  standalone: true,
  selector: 'app-donation-modal',
  templateUrl: './donation-modal.component.html',
  styleUrls: ['./donation-modal.component.scss'],
  imports: [CommonModule, FormsModule] // ✅ aqui está o segredo
})
export class DonationModalComponent {
  @Input() name!: string;
  @Input() cause!: string;
  @Input() show = false;

  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<number>();

  donationValue = 0;

  onConfirm() {
    this.confirm.emit(this.donationValue);
    this.donationValue = 0;
  }

  onClose() {
    this.close.emit();
    this.donationValue = 0;
  }
}
