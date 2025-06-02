import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div style="display: flex; justify-content: center; align-items: center; height: 80vh; flex-direction: column;">
      <p *ngIf="!errorMessage">Processando seu login...</p>
      <p *ngIf="errorMessage" style="color: red;">Erro: {{ errorMessage }}</p>
      <p *ngIf="errorMessage"><a routerLink="/login">Tentar login novamente</a></p>
    </div>
  `
})
export class AuthCallbackComponent implements OnInit {
  errorMessage: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const error = this.route.snapshot.queryParamMap.get('error');
    const message = this.route.snapshot.queryParamMap.get('message');

    if (error) {
      this.errorMessage = message || 'Ocorreu um erro durante a autenticação com o Google.';
      console.error(`Erro no callback do Google: ${error} - ${message}`);
    }

    const token = this.route.snapshot.queryParamMap.get('token');

    if (token) {
      this.authService.storeToken(token);
      console.log('Token do Google OAuth recebido e armazenado.');
      const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/dashboard';
      this.router.navigateByUrl(returnUrl);
    } else {
      this.errorMessage = 'Token não recebido do provedor de autenticação.';
      console.error('Token não encontrado na URL de callback do Google.');
      // setTimeout(() => this.router.navigate(['/login']), 5000);
    }
  }
}