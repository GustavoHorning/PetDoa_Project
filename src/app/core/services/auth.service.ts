import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; 
import { Observable, throwError } from 'rxjs'; 
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { RegisterDonorDto } from '../models/register-donor.dto';

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

  constructor(private http: HttpClient) { }

  /**
   * Registra um novo doador.
   * @param donorData Os dados do doador para registro.
   * @returns Um Observable com a resposta da API.
   */
  register(donorData: RegisterDonorDto): Observable<RegistrationResponse> {
    const registerUrl = `${this.apiUrlBase}/auth/donor/register`;
    return this.http.post<RegistrationResponse>(registerUrl, donorData).pipe(
      tap(response => console.log('Resposta do registro:', response)),
      catchError(this.handleError)
    );
  }

  // --- TODO: Implementar os métodos de Login futuramente ---
  // login(credentials: LoginDonorDto): Observable<LoginResponse> {
  //   const loginUrl = `${this.apiUrlBase}/auth/donor/login`;
  //   return this.http.post<LoginResponse>(loginUrl, credentials).pipe(
  //     tap(response => {
  //       console.log('Token recebido:', response.token);
  //       // this.storeToken(response.token); // Futuramente, armazenar o token
  //     }),
  //     catchError(this.handleError)
  //   );
  // }
  // googleLoginInitiate(): void {
  //   window.location.href = `${this.apiUrlBase}/auth/google/login`; // Simples redirecionamento
  // }
  // ... métodos para storeToken, getToken, logout, isAuthenticated ...

  private handleError(error: HttpErrorResponse) {
    let displayMessage = 'Ocorreu um erro ao processar sua solicitação. Tente novamente.';
    
    // Log do erro completo no console do navegador para depuração
    console.error('Backend error:', error);
    console.error('Error body:', JSON.stringify(error.error));

    if (error.error instanceof ErrorEvent) {
      // Erro do lado do cliente ou de rede.
      displayMessage = `Erro de rede ou cliente: ${error.error.message}`;
    } else {
      // O backend retornou um código de erro.
      if (error.status === 0) {
        displayMessage = 'Não foi possível conectar ao servidor. Verifique sua conexão ou a URL da API.';
      } else if (error.error) {
        // Caso 1: Erros de validação do ASP.NET Core (ValidationProblemDetails)
        // Onde error.error.errors é um objeto com campos como chaves e arrays de strings como valores
        if (error.status === 400 && error.error.errors && typeof error.error.errors === 'object' && !Array.isArray(error.error.errors)) {
          const fieldErrorMessages = [];
          for (const field in error.error.errors) {
            if (error.error.errors.hasOwnProperty(field) && Array.isArray(error.error.errors[field])) {
              fieldErrorMessages.push(error.error.errors[field].join(' ')); // Junta mensagens de um mesmo campo
            }
          }
          if (fieldErrorMessages.length > 0) {
            displayMessage = fieldErrorMessages.join('; ');
          } else if (typeof error.error.title === 'string') {
            displayMessage = error.error.title; // Título genérico do ValidationProblemDetails
          } else {
            displayMessage = "Foram encontrados erros de validação.";
          }
        }
        // Caso 2: Erros customizados do nosso backend onde error.error.errors é um array de strings
        else if (error.error.message && Array.isArray(error.error.errors) && error.error.errors.length > 0) {
          // Ex: { message: "A senha não atende...", errors: ["Deve ter maiúscula", "Deve ter número"] }
          displayMessage = `${error.error.message} ${error.error.errors.join('; ')}`;
        }
        // Caso 3: Erros customizados do nosso backend onde só temos error.error.message
        else if (typeof error.error.message === 'string') {
          displayMessage = error.error.message;
        }
        // Caso 4: O backend retornou uma string simples como erro
        else if (typeof error.error === 'string') {
           displayMessage = error.error;
        }
        // Fallback para outros erros do servidor
        else {
          displayMessage = `Erro ${error.status}: ${error.statusText || 'Ocorreu um erro no servidor.'}`;
        }
      } else {
        // Sem corpo de erro, usa o statusText
        displayMessage = `Erro ${error.status}: ${error.statusText || 'Ocorreu um erro no servidor.'}`;
      }
    }
    return throwError(() => new Error(displayMessage));
  }
}