import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, of, throwError } from 'rxjs';


export interface DashboardSummary {
  totalDonated: number;
  lastDonationDate: string | null;
  donorName: string;
  itemsDonatedCount: number;
}

export interface DonationHistory {
  id: number;
  date: string;
  amount: number;
  method: string;
  status: string;
  productName?: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  isActive: boolean;
}


@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = 'https://localhost:7016/api';

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  getSummary(): Observable<DashboardSummary> {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('authToken_petdoa_v1');
      const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
      return this.http.get<DashboardSummary>(`${this.apiUrl}/donation/my-summary`, { headers });
    } else {
      return of({
        totalDonated: 0,
        lastDonationDate: null,
        donorName: '',
        itemsDonatedCount: 0
      });
    }
  }

getHistory(page: number, pageSize: number): Observable<PaginatedResult<DonationHistory>> {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('authToken_petdoa_v1');
      if (!token) {
        return of({ items: [], totalCount: 0, pageNumber: 1, pageSize: pageSize, totalPages: 0 });
      }
      const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
      return this.http.get<PaginatedResult<DonationHistory>>(`${this.apiUrl}/donation/me?pageNumber=${page}&pageSize=${pageSize}`, { headers });
    } else {
      return of({ items: [], totalCount: 0, pageNumber: 1, pageSize: pageSize, totalPages: 0 });
    }
  }

      createPayment(amount: number, productId?: number): Observable<{ paymentUrl: string }> {
        if (isPlatformBrowser(this.platformId)) {
          const token = localStorage.getItem('authToken_petdoa_v1');
          if (!token) {
            console.error("Tentativa de pagamento sem token no localStorage.");
            return throwError(() => new Error('Sua sessão expirou. Por favor, faça login novamente.'));
          }

          const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
          const body = { amount, productId };

          return this.http.post<{ paymentUrl: string }>(`${this.apiUrl}/payments/checkout`, body, { headers });

        } else {
          console.error("createPayment foi chamado no lado do servidor, o que não deveria acontecer.");
          return throwError(() => new Error('A criação de pagamento não pode ser feita no servidor.'));
        }
  }


  getReceipt(donationId: number): Observable<Blob> {
  if (isPlatformBrowser(this.platformId)) {
    const token = localStorage.getItem('authToken_petdoa_v1');

    if (!token) {
      console.error("Token não encontrado ao tentar baixar recibo.");
      return throwError(() => new Error('Sua sessão expirou ou o token não foi encontrado.'));
    }

    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    return this.http.get(`${this.apiUrl}/donation/${donationId}/receipt`, {
      headers: headers,
      responseType: 'blob'
    });
  } else {
    return throwError(() => new Error('A geração de recibos só pode ser feita no navegador.'));
  }



}

  
getConsolidatedReport(startDate: string, endDate: string): Observable<Blob> {
  if (isPlatformBrowser(this.platformId)) {
    const token = localStorage.getItem('authToken_petdoa_v1');

    if (!token) {
      console.error("Token não encontrado ao gerar relatório.");
      return throwError(() => new Error('Usuário não autenticado.'));
    }

    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    
    const url = `${this.apiUrl}/donation/report?startDate=${startDate}&endDate=${endDate}`;

    return this.http.get(url, {
      headers: headers,
      responseType: 'blob'
    });

  } else {
    return throwError(() => new Error('Relatórios só podem ser gerados no navegador.'));
  }
}

getProducts(): Observable<Product[]> {
  if (isPlatformBrowser(this.platformId)) {
    const token = localStorage.getItem('authToken_petdoa_v1');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    return this.http.get<Product[]>(`${this.apiUrl}/products`, { headers });
  } else {
    return of([]);
  }
}


getRecentHistory(limit: number): Observable<DonationHistory[]> {
    if (isPlatformBrowser(this.platformId)) {
        const token = localStorage.getItem('authToken_petdoa_v1');
        if (!token) { return of([]); }
        
        const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
        return this.http.get<DonationHistory[]>(`${this.apiUrl}/donation/me/recent?limit=${limit}`, { headers });
    } else {
        return of([]);
    }
}
}
