import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, of, throwError } from 'rxjs';

export interface AdminDashboardStatsDto {
  revenueToday: number;
  revenueThisMonth: number;
  newDonorsThisMonth: number;
  donationsThisMonth: number;
}

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  isActive: boolean;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface AdminDonorList {
  id: number;
  name: string;
  email: string;
  registrationDate: string;
  donationCount: number;
  totalDonated: number;
}

export interface ProductDto {
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  isActive?: boolean;
}

export interface DailyRevenueDto {
  date: string; 
  amount: number;
}

export interface AdminDashboardDto {
  stats: AdminDashboardStatsDto;
  dailyRevenueLast30Days: DailyRevenueDto[];
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = 'https://localhost:7016/api';

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}


  private createAuthHeaders(): HttpHeaders | null {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('authToken_petdoa_v1');
      if (token) {
        return new HttpHeaders().set('Authorization', `Bearer ${token}`);
      }
    }
    return null;
  }

  getDashboardData(): Observable<AdminDashboardDto | null> {
    const headers = this.createAuthHeaders();
    if (!headers) return of(null); // Retorna nulo se estiver no servidor
    return this.http.get<AdminDashboardDto>(`${this.apiUrl}/administrator/dashboard`, { headers });
  }

  getAllProducts(): Observable<Product[]> {
    const headers = this.createAuthHeaders();
    if (!headers) return of([]); // Retorna array vazio se estiver no servidor
    return this.http.get<Product[]>(`${this.apiUrl}/products/all`, { headers });
  }

  createProduct(productDto: ProductDto): Observable<Product> {
    const headers = this.createAuthHeaders();
    if (!headers) return throwError(() => new Error('Usuário não autenticado.'));
    return this.http.post<Product>(`${this.apiUrl}/products`, productDto, { headers });
  }

  updateProduct(id: number, productDto: ProductDto): Observable<any> {
    const headers = this.createAuthHeaders();
    if (!headers) return throwError(() => new Error('Usuário não autenticado.'));
    return this.http.put(`${this.apiUrl}/products/${id}`, productDto, { headers });
  }

  deactivateProduct(id: number): Observable<any> {
    const headers = this.createAuthHeaders();
    if (!headers) return throwError(() => new Error('Usuário não autenticado.'));
    return this.http.delete(`${this.apiUrl}/products/${id}`, { headers });
  }

  getDonors(page: number, pageSize: number, searchTerm: string = ''): Observable<PaginatedResult<AdminDonorList>> {
    const headers = this.createAuthHeaders();
    if (!headers) return of({ items: [], totalCount: 0, pageNumber: 1, pageSize: pageSize, totalPages: 0 });

    const url = `${this.apiUrl}/administrator/donors?pageNumber=${page}&pageSize=${pageSize}&searchTerm=${searchTerm}`;
    return this.http.get<PaginatedResult<AdminDonorList>>(url, { headers });
  }

  exportDonations(startDate: string, endDate: string): Observable<Blob> {
    const headers = this.createAuthHeaders();
    if (!headers) return throwError(() => new Error('Usuário não autenticado.'));
    
    const url = `${this.apiUrl}/donation/export?startDate=${startDate}&endDate=${endDate}`;
    
    return this.http.get(url, {
      headers,
      responseType: 'blob'
    });
}
}
