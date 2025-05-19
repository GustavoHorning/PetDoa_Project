import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { InstitutionCardComponent } from '../../shared/institution-card/institution-card.component';
import { CardInfoComponent } from '../../shared/card-info/card-info.component';
import { DonationModalComponent } from '../../shared/donation-modal/donation-modal.component';
import { Institution } from '../../core/models/institution.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  
  imports: [
    CommonModule,
    FormsModule,
    CardInfoComponent,
    InstitutionCardComponent,
    DonationModalComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {

  institutions: Institution[] = [
    { name: 'ONG Amigos dos Animais', cause: 'Resgate de rua' },
    { name: 'Clínica Vet Esperança', cause: 'Tratamento emergencial' },
    { name: 'Lar Pet Feliz', cause: 'Adoção responsável' }
  ];


  selectedInstitution: Institution | null = null;

  modalVisible = false;

  handleDonation(inst: Institution) {
    this.selectedInstitution = inst;
    this.modalVisible = true;
  }

  handleModalClose() {
    this.modalVisible = false;
    this.selectedInstitution = null;
  }

  handleDonationConfirm(value: number) {
    alert(`Doação confirmada: R$ ${value.toFixed(2)} para ${this.selectedInstitution?.name}`);
    this.modalVisible = false;
    this.selectedInstitution = null;
  }
}
