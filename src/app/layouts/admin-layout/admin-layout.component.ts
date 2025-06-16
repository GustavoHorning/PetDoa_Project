import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router'; // 1. Importe o RouterModule
import { AuthService } from '../../core/services/auth.service'; // 2. Importe o AuthService

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule], // 3. Adicione o RouterModule aos imports
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss']
})
export class AdminLayoutComponent {

  // 4. Injete o AuthService para poder usá-lo
  constructor(private authService: AuthService) {}

  // 5. Crie o método de logout que estava faltando
  logout(): void {
    this.authService.logout();
  }
}