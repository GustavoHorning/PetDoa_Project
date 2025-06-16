import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { DashboardService, DashboardSummary, DonationHistory } from '../../core/services/dashboard.service';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';



@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, FormsModule, RouterModule],
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

  currentPage = 1;
  pageSize = 5; 
  totalPages = 0;

  isReportModalVisible = false;
  reportStartDate: string = '';
  reportEndDate: string = '';
  isGeneratingReport = false; 
  public today: string = new Date().toISOString().split('T')[0];


  constructor(private dashboardService: DashboardService, private cdr: ChangeDetectorRef, private authService: AuthService, private router: Router) {}

  
ngOnInit(): void {
  this.dashboardService.getSummary().subscribe(data => {
    this.summary = data;
    this.isLoading = false;
    this.timeSinceLastDonation = this.calculateTimeSinceLastDonation();
  });

  this.loadHistory(this.currentPage);
}


loadHistory(page: number): void {
    this.dashboardService.getHistory(page, this.pageSize).subscribe(result => {
      this.history = result.items;
      this.currentPage = result.pageNumber;
      this.totalPages = result.totalPages;
    });
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.loadHistory(this.currentPage + 1);
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.loadHistory(this.currentPage - 1);
    }
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
    return 'Não informado';
  }
  switch (method.toLowerCase()) {
    case 'creditcard': // <-- CORRIGIDO
      return 'Cartão de Crédito';
    case 'pix':
      return 'Pix';
    case 'boleto':
      return 'Boleto';
    case 'outro':
      return 'Outro';
    default:
      return method;
  }
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
    this.cdr.detectChanges();
  }

  closeDonationModal(): void {
    this.isDonationModalVisible = false;
    this.donationAmount = 25; 
    this.cdr.detectChanges();
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


  downloadReceipt(donationId: number): void {
  
  this.dashboardService.getReceipt(donationId).subscribe({
    next: (blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `recibo-doacao-${donationId}.pdf`;
      
      document.body.appendChild(a);
      a.click();
      
      window.URL.revokeObjectURL(url);
      a.remove();
    },
    error: (err) => {
      console.error('Erro ao baixar o recibo:', err);
      alert('Não foi possível gerar o recibo. Tente novamente mais tarde.');
    }
  });
}


openReportModal(): void {
    this.setReportRange('thisMonth');
    this.isReportModalVisible = true;
    this.cdr.detectChanges();
  }

  closeReportModal(): void {
    this.isReportModalVisible = false;
    this.cdr.detectChanges(); 
  }


  setReportRange(period: 'thisMonth' | 'lastMonth'): void {
    const now = new Date();
    let startDate: Date;
    let endDate: Date;

    if (period === 'thisMonth') {
        startDate = new Date(now.getFullYear(), now.getMonth(), 1);
        endDate = now;
    } else {
        startDate = new Date(now.getFullYear(), now.getMonth() - 1, 1);
        endDate = new Date(now.getFullYear(), now.getMonth(), 0);
    }
    
    this.reportStartDate = startDate.toISOString().split('T')[0];
    this.reportEndDate = endDate.toISOString().split('T')[0];
  }


  generateReport(): void {
    if (!this.reportStartDate || !this.reportEndDate) {
      alert('Por favor, selecione um período de datas válido.');
      return;
    }
    
    this.isGeneratingReport = true;

    this.dashboardService.getConsolidatedReport(this.reportStartDate, this.reportEndDate).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `relatorio-doacoes-${this.reportStartDate}-a-${this.reportEndDate}.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
        
        this.isGeneratingReport = false;
        this.closeReportModal();
      },
      error: (err) => {
        console.error('Erro ao gerar relatório:', err);
        alert('Não foi possível gerar o relatório. Tente novamente.');
        this.isGeneratingReport = false;
      }
    });
  }

  public logout(): void {
  this.authService.logout();

  this.router.navigate(['/login']);
}
}