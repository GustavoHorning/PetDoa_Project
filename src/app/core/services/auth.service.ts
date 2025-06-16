import { Injectable, PLATFORM_ID, Inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { RegisterDonorDto } from '../models/register-donor.dto';
import { LoginDonorDto } from '../models/login-donor.dto';
import { DonorReadDTO, UpdateDonorProfileDto, ChangePasswordDto } from '../models/donor.dtos'; // Supondo que DonorReadDTO, UpdateDonorProfileDto e ChangePasswordDto estão em donor.dtos.ts
import { Router } from '@angular/router';
import { AdminLoginDto } from '../models/admin.dtos.ts';
import { isPlatformBrowser } from '@angular/common';



interface RegistrationResponse {
  message: string;
  token?: string;
}

interface LoginResponse {
  token: string;
  role: string;
}

interface AdminLoginResponse {
  token: string;
  role: string;
}

interface ChangePasswordResponse {
    message: string;
}

interface UploadPictureResponse {
    profilePictureUrl: string;
}

interface DonorLoginResponse {
  token: string;
}


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrlBase = environment.apiUrl;
  private readonly TOKEN_KEY = 'authToken_petdoa_v1';
  private readonly ROLE_KEY = 'userRole_petdoa_v1'; 


  private loggedInStatus = new BehaviorSubject<boolean>(this.hasToken());
  public isLoggedIn$ = this.loggedInStatus.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: object
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

  storeToken(token: string): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.TOKEN_KEY, token);
      this.updateAuthenticationState(true);
    }
  }

  getToken(): string | null {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem(this.TOKEN_KEY);
    }
    return null;
  }

  logout(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.ROLE_KEY);
    }
    this.updateAuthenticationState(false);
    this.router.navigate(['/login']);
  }

  public getUserRole(): string | null {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem(this.ROLE_KEY);
    }
    return null;
  }

  register(donorData: RegisterDonorDto): Observable<RegistrationResponse> {
    const registerUrl = `${this.apiUrlBase}/auth/donor/register`;
    return this.http.post<RegistrationResponse>(registerUrl, donorData).pipe(
      tap(response => {
        console.log('Resposta do registro:', response);
        if (response && response.token) {
          this.storeToken(response.token);
          console.log('Token do registro armazenado. Usuário auto-logado.');
        }
      }),
      catchError(this.handleError)
    );
  }


  // login(credentials: LoginDonorDto): Observable<LoginResponse> {
  //   const loginUrl = `${this.apiUrlBase}/auth/donor/login`;
  //   return this.http.post<LoginResponse>(loginUrl, credentials).pipe(
  //     tap(response => {
  //       if (response && response.token) {
  //         this.storeToken(response.token);
  //         console.log('Login bem-sucedido. Token armazenado.');
  //       } else {
  //         console.error('Resposta de login bem-sucedida, mas sem token recebido:', response);
  //       }
  //     }),
  //     catchError(this.handleError)
  //   );
  // }

  loginDonor(credentials: LoginDonorDto): Observable<LoginResponse> {
    const loginUrl = `${this.apiUrlBase}/auth/donor/login`;

    return this.http.post<LoginResponse>(loginUrl, credentials).pipe(
      tap(response => {        
        if (response && response.token) {
          this.storeToken(response.token);
        }
      }),
      catchError(this.handleError)
    );
  }

  // loginAdmin(credentials: AdminLoginDto): Observable<LoginResponse> {
  //   // Verifique se a rota de autenticação do seu AdminController está correta
  //   const loginUrl = `${this.apiUrlBase}/auth/admin/login`; 
  //   return this.http.post<LoginResponse>(loginUrl, credentials).pipe(
  //     tap(response => {        
  //       if (response && response.token) {
  //         this.storeToken(response.token);
  //       }
  //       }),
  //     catchError(this.handleError)
  //   );
  // }

  loginAdmin(credentials: AdminLoginDto): Observable<AdminLoginResponse> {
    console.log('%c[AuthService] Tentando login de ADMIN...', 'color: blue;');
    // CORREÇÃO APLICADA AQUI: Usando a rota que funciona no Postman.
    const loginUrl = `${this.apiUrlBase}/auth/admin/login`;
    return this.http.post<AdminLoginResponse>(loginUrl, credentials).pipe(
      tap(response => {
        console.log('%c[AuthService] Resposta de SUCESSO para Admin recebida.', 'color: green;');
        if (response && response.token && response.role) {
          this.storeSession(response.token, response.role); // Salvamos o token e a role que veio da API
        }
      }),
      catchError(this.handleError)
    );
  }

  // loginDonor(credentials: LoginDonorDto): Observable<DonorLoginResponse> {
  //   console.log('%c[AuthService] Tentando login de DOADOR...', 'color: blue;');
  //   const loginUrl = `${this.apiUrlBase}/auth/donor/login`;
  //   return this.http.post<DonorLoginResponse>(loginUrl, credentials).pipe(
  //     tap(response => {
  //       console.log('%c[AuthService] Resposta de SUCESSO para Doador recebida.', 'color: green;');
  //       if (response && response.token) {
  //         this.storeSession(response.token, 'Donor'); // Salvamos o token e definimos a role como 'Donor'
  //       }
  //     }),
  //     catchError(this.handleError)
  //   );
  // }

  private storeSession(token: string, role: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.TOKEN_KEY, token);
      localStorage.setItem(this.ROLE_KEY, role);
      this.updateAuthenticationState(true);
      console.log(`%c[AuthService] Sessão salva. Token armazenado. Role: ${role}`, 'color: green;');
    }
  }

  private saveSession(response: LoginResponse): void {
    if (typeof localStorage !== 'undefined') {
      if (response && response.token && response.role) {
        localStorage.setItem(this.TOKEN_KEY, response.token);
        localStorage.setItem(this.ROLE_KEY, response.role); // Salva a role
        this.updateAuthenticationState(true);
        console.log('Sessão salva com sucesso. Token e Role armazenados.');
      }
    }
  }

  initiateGoogleAuth(): void {
    const googleLoginUrl = `${this.apiUrlBase}/auth/google/login`;
    if (typeof window !== 'undefined') {
      console.log('Redirecionando para o login do Google...');
      window.location.href = googleLoginUrl;
    }
  }

  getUserProfile(): Observable<DonorReadDTO> {
    const profileUrl = `${this.apiUrlBase}/donor/me`;
    // NOTA: Requer token JWT no header (HttpInterceptor idealmente)
    return this.http.get<DonorReadDTO>(profileUrl).pipe(
      catchError(this.handleError)
    );
  }


  updateProfileName(data: UpdateDonorProfileDto): Observable<void> {
    const updateUrl = `${this.apiUrlBase}/donor/me`;
    return this.http.put<void>(updateUrl, data).pipe(
      catchError(this.handleError)
    );
  }

  changePassword(data: ChangePasswordDto): Observable<ChangePasswordResponse> {
    const changePasswordUrl = `${this.apiUrlBase}/donor/me/password`;
    return this.http.put<ChangePasswordResponse>(changePasswordUrl, data).pipe(
      catchError(this.handleError)
    );
  }

  uploadProfilePicture(file: File): Observable<UploadPictureResponse> {
    const uploadUrl = `${this.apiUrlBase}/donor/me/picture`;
    const formData = new FormData();
    formData.append('imageFile', file, file.name);

    return this.http.post<UploadPictureResponse>(uploadUrl, formData).pipe(
      catchError(this.handleError)
    );
  }

  deleteProfilePicture(): Observable<void> {
    const deleteUrl = `${this.apiUrlBase}/donor/me/picture`;
    return this.http.delete<void>(deleteUrl).pipe(
      catchError(this.handleError)
    );
  }

private handleError = (error: HttpErrorResponse): Observable<never> => {
  console.log('%c[AuthService] ERRO na chamada HTTP:', 'color: red; font-weight: bold;');
    console.log(`%c--> URL da Requisição: ${error.url}`, 'color: red;');
    console.log(`%c--> Status do Erro: ${error.status}`, 'color: red;');
    console.log(`%c--> Corpo do Erro (RAW):`, 'color: red;', error.error);
  let displayMessage = 'Ocorreu um erro ao processar sua solicitação. Tente novamente.';

  console.error('Backend error:', error);
  console.error('Error body:', JSON.stringify(error.error));

  if (error.error instanceof ErrorEvent) {
    displayMessage = `Erro de rede ou cliente: ${error.error.message}`;
  } else {
    if (error.status === 401) { 
      if (error.url?.includes('/auth/donor/login') || error.url?.includes('/auth/admin/login')) {
        displayMessage = error.error?.message || 'E-mail ou senha inválidos.';
      } else {
        displayMessage = 'Sua sessão expirou ou é inválida. Por favor, faça login novamente.';
        this.logout();
      }
    } else if (error.status === 0) {
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