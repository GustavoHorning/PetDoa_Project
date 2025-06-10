import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { DashboardService, DashboardSummary, DonationHistory } from '../../core/services/dashboard.service';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  summary: DashboardSummary | null = null;
  history: DonationHistory[] = [];
  isLoading = true; 
  isDonationModalVisible = false;
  donationAmount: number | null = 25;
  isProcessingDonation = false;
  intervalId: any;
  timeSinceLastDonation: string = '';

  


  constructor(private dashboardService: DashboardService) {}

  
ngOnInit(): void {
  this.dashboardService.getSummary().subscribe(data => {
    this.summary = data;
    this.isLoading = false;
    this.timeSinceLastDonation = this.calculateTimeSinceLastDonation();
  });

  this.dashboardService.getHistory().subscribe(data => {
    this.history = data;
  });
}


  private calculateTimeSinceLastDonation(): string {

  if (!this.summary?.lastDonationDate) {
    return 'Nenhuma doação encontrada';
  }

  const lastDate = new Date(this.summary.lastDonationDate + 'Z');
  const now = new Date();
  const diffInSeconds = Math.floor((now.getTime() - lastDate.getTime()) / 1000);

  if (diffInSeconds < 60) {
    return 'Agora mesmo';
  } else if (diffInSeconds < 3600) {
    const minutes = Math.floor(diffInSeconds / 60);
    return `${minutes} minuto${minutes > 1 ? 's' : ''} atrás`;
  } else if (diffInSeconds < 86400) {
    const hours = Math.floor(diffInSeconds / 3600);
    return `${hours} hora${hours > 1 ? 's' : ''} atrás`;
  } else if (diffInSeconds < 172800) {
    return 'Ontem';
  } else {
    const days = Math.floor(diffInSeconds / 86400);
    return `${days} dias atrás`;
  }
}



  public formatPaymentMethod(method: string): string {
    if (!method) {
      return '';
    }
    return method.replace('_', ' ');
  }

  public getStatusInfo(status: any): { text: string; cssClass: string } {
  switch (status) {
    case 'Completed':
      return { text: 'Aprovado', cssClass: 'status--approved' };
    case 'Pending':
      return { text: 'Pendente', cssClass: 'status--pending' };
    case 'Failed':
      return { text: 'Falhou', cssClass: 'status--failed' };
    default:
      return { text: 'Desconhecido', cssClass: 'status--unknown' };
  }
  }


  openDonationModal(): void {
    this.isDonationModalVisible = true;
  }

  closeDonationModal(): void {
    this.isDonationModalVisible = false;
    this.donationAmount = 25; 
  }

  setDonationAmount(amount: number): void {
    this.donationAmount = amount;
  }

  confirmDonation(): void {
    if (!this.donationAmount || this.donationAmount <= 0) {
      alert('Por favor, insira um valor de doação válido.');
      return;
    }

    this.isProcessingDonation = true;

    this.dashboardService.createPayment(this.donationAmount).subscribe({
      next: (response) => {
        window.open(response.paymentUrl, '_blank');
        this.isProcessingDonation = false;
        this.closeDonationModal();
      },
      error: (err) => {
        console.error('Erro ao criar pagamento:', err);
        alert('Ocorreu um erro ao iniciar sua doação. Tente novamente.');
        this.isProcessingDonation = false;
      }
    });
  }
}