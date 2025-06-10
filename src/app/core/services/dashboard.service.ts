// dashboard.service.ts
import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, of, throwError } from 'rxjs';


export interface DashboardSummary {
  totalDonated: number;
  lastDonationDate: string | null;
  donorName: string;
}

export interface DonationHistory {
  date: string;
  amount: number;
  method: string;
  status: string;
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
        donorName: ''
      });
    }
  }

  getHistory(): Observable<DonationHistory[]> {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('authToken_petdoa_v1');
      const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
      return this.http.get<DonationHistory[]>(`${this.apiUrl}/donation/me`, { headers });
    } else {
      return of([]);
    }
  }

      createPayment(amount: number): Observable<{ paymentUrl: string }> {
        if (isPlatformBrowser(this.platformId)) {
          const token = localStorage.getItem('authToken_petdoa_v1');
          if (!token) {
            console.error("Tentativa de pagamento sem token no localStorage.");
            return throwError(() => new Error('Sua sessão expirou. Por favor, faça login novamente.'));
          }

          const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
          const body = { amount };

          return this.http.post<{ paymentUrl: string }>(`${this.apiUrl}/payments/checkout`, body, { headers });

        } else {
          console.error("createPayment foi chamado no lado do servidor, o que não deveria acontecer.");
          return throwError(() => new Error('A criação de pagamento não pode ser feita no servidor.'));
        }
  }
}
