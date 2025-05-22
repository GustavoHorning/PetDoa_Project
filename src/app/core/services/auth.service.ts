import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { RegisterDonorDto } from '../models/register-donor.dto';
import { LoginDonorDto } from '../models/login-donor.dto';
import { Router } from '@angular/router';

interface RegistrationResponse {
  message: string;
}

interface LoginResponse {
  token: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrlBase = environment.apiUrl;
  private readonly TOKEN_KEY = 'authToken_petdoa_v1';

  private loggedInStatus = new BehaviorSubject<boolean>(this.hasToken());
  public isLoggedIn$ = this.loggedInStatus.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  
  private hasToken(): boolean {
    if (typeof localStorage !== 'undefined') {
      return !!localStorage.getItem(this.TOKEN_KEY);
    }
    return false;
  }

  
  private updateAuthenticationState(isLoggedIn: boolean): void {
    this.loggedInStatus.next(isLoggedIn);
  }

  /**
   * Armazena o token JWT no localStorage e atualiza o estado de login.
   * @param token O token JWT recebido do backend.
   */
  storeToken(token: string): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.TOKEN_KEY, token);
      this.updateAuthenticationState(true);
    }
  }

  /**
   * Obtém o token JWT do localStorage.
   * @returns O token JWT ou null se não estiver armazenado.
   */
  getToken(): string | null {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem(this.TOKEN_KEY);
    }
    return null;
  }

  logout(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(this.TOKEN_KEY);
    }
    this.updateAuthenticationState(false);
    this.router.navigate(['/login']);
  }

  register(donorData: RegisterDonorDto): Observable<RegistrationResponse> {
    const registerUrl = `${this.apiUrlBase}/auth/donor/register`;
    return this.http.post<RegistrationResponse>(registerUrl, donorData).pipe(
      tap(response => console.log('Resposta do registro:', response)),
      catchError(this.handleError)
    );
  }


  login(credentials: LoginDonorDto): Observable<LoginResponse> {
    const loginUrl = `${this.apiUrlBase}/auth/donor/login`;
    return this.http.post<LoginResponse>(loginUrl, credentials).pipe(
      tap(response => {
        if (response && response.token) {
          this.storeToken(response.token);
          console.log('Login bem-sucedido. Token armazenado.');
        } else {
          console.error('Resposta de login bem-sucedida, mas sem token recebido:', response);
        }
      }),
      catchError(this.handleError)
    );
  }

  initiateGoogleAuth(): void {
    const googleLoginUrl = `${this.apiUrlBase}/auth/google/login`;
    if (typeof window !== 'undefined') {
      console.log('Redirecionando para o login do Google...');
      window.location.href = googleLoginUrl;
    }
  }

  private handleError(error: HttpErrorResponse) {
    let displayMessage = 'Ocorreu um erro ao processar sua solicitação. Tente novamente.';
    console.error('Backend error:', error);
    console.error('Error body:', JSON.stringify(error.error));

    if (error.error instanceof ErrorEvent) {
      displayMessage = `Erro de rede ou cliente: ${error.error.message}`;
    } else {
      if (error.status === 0) {
        displayMessage = 'Não foi possível conectar ao servidor. Verifique sua conexão ou a URL da API.';
      } else if (error.error) {
        if (error.status === 400 && error.error.errors && typeof error.error.errors === 'object' && !Array.isArray(error.error.errors)) {
          const fieldErrorMessages = [];
          for (const field in error.error.errors) {
            if (error.error.errors.hasOwnProperty(field) && Array.isArray(error.error.errors[field])) {
              fieldErrorMessages.push(error.error.errors[field].join(' '));
            }
          }
          if (fieldErrorMessages.length > 0) {
            displayMessage = fieldErrorMessages.join('; ');
          } else if (typeof error.error.title === 'string') {
            displayMessage = error.error.title;
          } else {
            displayMessage = "Foram encontrados erros de validação.";
          }
        }
        else if (error.error.message && Array.isArray(error.error.errors) && error.error.errors.length > 0) {
          displayMessage = `${error.error.message} ${error.error.errors.join('; ')}`;
        }
        else if (typeof error.error.message === 'string') {
          displayMessage = error.error.message;
        }
        else if (typeof error.error === 'string') {
           displayMessage = error.error;
        }
        else {
          displayMessage = `Erro ${error.status}: ${error.statusText || 'Ocorreu um erro no servidor.'}`;
        }
      } else {
        displayMessage = `Erro ${error.status}: ${error.statusText || 'Ocorreu um erro no servidor.'}`;
      }
    }
    return throwError(() => new Error(displayMessage));
  }
}