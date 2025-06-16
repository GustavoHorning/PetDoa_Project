import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { AdminService, AdminDonorList, PaginatedResult } from '../../../core/services/admin.service';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';


@Component({
  selector: 'app-donor-management',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './donor-management.component.html',
  styleUrls: ['./donor-management.component.scss']
})
export class DonorManagementComponent implements OnInit, OnDestroy {
  donors: AdminDonorList[] = [];
  isLoading = true;

  // Paginação
  currentPage = 1;
  pageSize = 5;
  totalPages = 0;

  // Busca
  private searchSubject = new Subject<string>();
  private searchSubscription!: Subscription;
  public searchTerm = '';

  // --- NOVAS PROPRIEDADES PARA O RELATÓRIO ---
  reportStartDate: string = '';
  reportEndDate: string = '';
  isExporting = false;
  public today: string = new Date().toISOString().split('T')[0];

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    const now = new Date();
    this.reportStartDate = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0];
    this.reportEndDate = this.today;
    this.loadDonors();

    // Lógica de busca com "debounce" para não chamar a API a cada tecla digitada
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(500), // Espera 500ms após o usuário parar de digitar
      distinctUntilChanged() // Só busca se o termo for diferente do anterior
    ).subscribe(term => {
      this.searchTerm = term;
      this.currentPage = 1; // Reseta para a primeira página a cada nova busca
      this.loadDonors();
    });
  }

  ngOnDestroy(): void {
    this.searchSubscription.unsubscribe();
  }

  

  loadDonors(): void {
    this.isLoading = true;
    this.adminService.getDonors(this.currentPage, this.pageSize, this.searchTerm).subscribe(result => {
      this.donors = result.items;
      this.totalPages = result.totalPages;
      this.currentPage = result.pageNumber;
      this.isLoading = false;
    });
  }


  onSearch(event: Event): void {
    const term = (event.target as HTMLInputElement).value;
    this.searchSubject.next(term);
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadDonors();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadDonors();
    }
  }
  

  exportToCsv(): void {
    if (!this.reportStartDate || !this.reportEndDate) {
      alert('Por favor, selecione um período de datas válido.');
      return;
    }
    this.isExporting = true;
    
    this.adminService.exportDonations(this.reportStartDate, this.reportEndDate).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `relatorio-doacoes-${this.reportStartDate}-a-${this.reportEndDate}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
        this.isExporting = false;
      },
      error: (err) => {
        alert('Ocorreu um erro ao gerar o relatório.');
        this.isExporting = false;
      }
    });
  }
}
