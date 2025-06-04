import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
// Certifique-se que os caminhos e nomes de arquivos dos DTOs estão corretos:
import { RegisterDonorDto } from '../models/register-donor.dto';
import { LoginDonorDto } from '../models/login-donor.dto';
import { DonorReadDTO, UpdateDonorProfileDto, ChangePasswordDto } from '../models/donor.dtos'; // Supondo que DonorReadDTO, UpdateDonorProfileDto e ChangePasswordDto estão em donor.dtos.ts
import { Router } from '@angular/router';

interface RegistrationResponse {
  message: string;
  token?: string;
}

interface LoginResponse {
  token: string;
}

// Interface para a resposta do backend ao trocar senha (se ele envia uma mensagem customizada)
interface ChangePasswordResponse {
    message: string;
}

// Interface para a resposta do backend ao fazer upload da foto
interface UploadPictureResponse {
    profilePictureUrl: string;
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
    }
    this.updateAuthenticationState(false);
    this.router.navigate(['/login']);
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

  getUserProfile(): Observable<DonorReadDTO> {
    const profileUrl = `${this.apiUrlBase}/donor/me`;
    // NOTA: Requer token JWT no header (HttpInterceptor idealmente)
    return this.http.get<DonorReadDTO>(profileUrl).pipe(
      catchError(this.handleError)
    );
  }

  // --- NOVOS MÉTODOS PARA O PERFIL ---

  updateProfileName(data: UpdateDonorProfileDto): Observable<void> {
    const updateUrl = `${this.apiUrlBase}/donor/me`;
    // NOTA: Requer token JWT no header
    return this.http.put<void>(updateUrl, data).pipe(
      catchError(this.handleError)
    );
  }

  changePassword(data: ChangePasswordDto): Observable<ChangePasswordResponse> { // Mudado para ChangePasswordResponse
    const changePasswordUrl = `${this.apiUrlBase}/donor/me/password`;
    // NOTA: Requer token JWT no header
    return this.http.put<ChangePasswordResponse>(changePasswordUrl, data).pipe(
      catchError(this.handleError)
    );
  }

  uploadProfilePicture(file: File): Observable<UploadPictureResponse> {
    const uploadUrl = `${this.apiUrlBase}/donor/me/picture`;
    const formData = new FormData();
    // O backend espera um parâmetro chamado 'imageFile'
    formData.append('imageFile', file, file.name);

    // NOTA: Requer token JWT no header
    return this.http.post<UploadPictureResponse>(uploadUrl, formData).pipe(
      catchError(this.handleError)
    );
  }

  deleteProfilePicture(): Observable<void> {
    const deleteUrl = `${this.apiUrlBase}/donor/me/picture`;
    // NOTA: Requer token JWT no header
    return this.http.delete<void>(deleteUrl).pipe(
      catchError(this.handleError)
    );
  }

  // --- FIM DOS NOVOS MÉTODOS PARA O PERFIL ---

//   private handleError(error: HttpErrorResponse) {
//     // ... (seu método handleError existente e robusto) ...
//     let displayMessage = 'Ocorreu um erro ao processar sua solicitação. Tente novamente.';
//     console.error('Backend error:', error);
//     console.error('Error body:', JSON.stringify(error.error));

//     if (error.error instanceof ErrorEvent) {
//       displayMessage = `Erro de rede ou cliente: ${error.error.message}`;
//     } else {
//       if (error.status === 401) {
//         displayMessage = 'Sua sessão expirou ou é inválida. Por favor, faça login novamente.';
//         this.logout();
//       }
      
//       else if (error.status === 0) {
//         displayMessage = 'Não foi possível conectar ao servidor. Verifique sua conexão ou a URL da API.';
//       } else if (error.error) {
//         if (error.status === 400 && error.error.errors && typeof error.error.errors === 'object' && !Array.isArray(error.error.errors)) {
//           const fieldErrorMessages = [];
//           for (const field in error.error.errors) {
//             if (error.error.errors.hasOwnProperty(field) && Array.isArray(error.error.errors[field])) {
//               fieldErrorMessages.push(error.error.errors[field].join(' '));
//             }
//           }
//           if (fieldErrorMessages.length > 0) {
//             displayMessage = fieldErrorMessages.join('; ');
//           } else if (typeof error.error.title === 'string') {
//             displayMessage = error.error.title;
//           } else {
//             displayMessage = "Foram encontrados erros de validação.";
//           }
//         }
//         else if (error.error.message && Array.isArray(error.error.errors) && error.error.errors.length > 0) {
//           displayMessage = `${error.error.message} ${error.error.errors.join('; ')}`;
//         }
//         else if (typeof error.error.message === 'string') {
//           displayMessage = error.error.message;
//         }
//         else if (typeof error.error === 'string') {
//            displayMessage = error.error;
//         }
//         else {
//           displayMessage = `Erro ${error.status}: ${error.statusText || 'Ocorreu um erro no servidor.'}`;
//         }
//       } else {
//         displayMessage = `Erro ${error.status}: ${error.statusText || 'Ocorreu um erro no servidor.'}`;
//       }
//     }
//     return throwError(() => new Error(displayMessage));
//   }
// }


// DEPOIS (transforme em uma arrow function property):
private handleError = (error: HttpErrorResponse): Observable<never> => {
  // 'this' aqui dentro agora sempre se referirá à instância de AuthService
  let displayMessage = 'Ocorreu um erro ao processar sua solicitação. Tente novamente.';

  console.error('Backend error:', error);
  console.error('Error body:', JSON.stringify(error.error));

  if (error.error instanceof ErrorEvent) {
    displayMessage = `Erro de rede ou cliente: ${error.error.message}`;
  } else {
    if (error.status === 401) { // Não Autorizado (token inválido/expirado OU login/senha errados)
      // Se o erro 401 veio de uma tentativa de login (rota de login), a mensagem deve ser sobre credenciais.
      // Se veio de outra rota protegida, pode ser sessão expirada.
      // Vamos ajustar a mensagem que o LoginComponent exibe.
      // O logout aqui é para o caso de um token inválido/expirado ser detectado em chamadas subsequentes.
      // Para uma falha de login inicial, o logout não muda muito, mas não prejudica.
      if (error.url?.includes('/auth/donor/login') || error.url?.includes('/auth/admin/login')) {
        displayMessage = error.error?.message || 'E-mail ou senha inválidos.'; // Usa a mensagem do backend se existir
      } else {
        displayMessage = 'Sua sessão expirou ou é inválida. Por favor, faça login novamente.';
        this.logout(); // Faz logout se for um 401 em uma rota protegida (token expirado)
      }
    } else if (error.status === 0) {
      displayMessage = 'Não foi possível conectar ao servidor. Verifique sua conexão ou a URL da API.';
    } else if (error.error) {
      // Validação do ASP.NET Core (ValidationProblemDetails)
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
      // Erros customizados do backend com array de erros
      else if (error.error.message && Array.isArray(error.error.errors) && error.error.errors.length > 0) {
        displayMessage = `${error.error.message} ${error.error.errors.join('; ')}`;
      }
      // Erros customizados do backend com apenas message
      else if (typeof error.error.message === 'string') {
        displayMessage = error.error.message;
      }
      // Backend retornou uma string simples
      else if (typeof error.error === 'string') {
        displayMessage = error.error;
      }
      // Fallback
      else {
        displayMessage = `Erro ${error.status}: ${error.statusText || 'Ocorreu um erro no servidor.'}`;
      }
    } else {
      displayMessage = `Erro ${error.status}: ${error.statusText || 'Ocorreu um erro no servidor.'}`;
    }
  }
  return throwError(() => new Error(displayMessage)); // Retorna um Observable de erro com a mensagem formatada
}
}