import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common'; 
import { FormsModule } from '@angular/forms';
import { DashboardService, Product } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe], 
  templateUrl: './shop.component.html',
  styleUrls: ['./shop.component.scss']
})
export class ShopComponent implements OnInit {
  products: Product[] = [];
  isLoading = true;
  
  processingProductId: number | null = null; 

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getProducts().subscribe({
      next: (data) => { this.products = data; },
      error: (err) => { console.error('Erro ao carregar produtos:', err); },
      complete: () => { this.isLoading = false; }
    });
  }

  donateItem(product: Product): void {
    this.processingProductId = product.id; 

    this.dashboardService.createPayment(product.price, product.id).subscribe({
      next: (response) => {
        window.open(response.paymentUrl, '_blank');
        this.processingProductId = null;
      },
      error: (err) => {
        console.error('Erro ao criar pagamento:', err);
        alert('Ocorreu um erro ao iniciar sua doação. Tente novamente.');
        this.processingProductId = null; 
      }
    });
  }
}