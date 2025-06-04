import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../core/services/auth.service';
import { Subscription } from 'rxjs';
import { DonorReadDTO } from '../core/models/donor.dtos';
import { ProfileModalComponent } from '../features/user-profile/profile-modal/profile-modal.component';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, ProfileModalComponent],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {

  isUserLoggedIn = false;
  userName: string | null = null;
  userImageUrl: string | null = 'assets/img/avatar-default.png';
  //showUserModal = false;
  
  currentUserProfile: DonorReadDTO | null = null;

  // NOVAS PROPRIEDADES PARA DROPDOWN E MODAL DE EDIÇÃO
  showProfileDropdown = false;
  showProfileEditModal = false; // Controla o modal de edição de perfil

  userInitials: string | null = null;
  avatarBackgroundColor: string = '#cccccc';

  toastVisible = false;
  toastMessage = '';

  private authSubscription!: Subscription;

  private readonly avatarColors = [
    '#F44336', '#E91E63', '#9C27B0', '#673AB7', '#3F51B5',
    '#2196F3', '#03A9F4', '#00BCD4', '#009688', '#4CAF50',
    '#8BC34A', '#CDDC39', '#FFC107', '#FF9800', '#FF5722',
    '#795548', '#607D8B', '#4DB6AC', '#AED581', '#FFB74D', '#A1887F'
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authSubscription = this.authService.isLoggedIn$.subscribe(isLoggedIn => {
      this.isUserLoggedIn = isLoggedIn;
      if (isLoggedIn) {
        this.loadUserProfile();
      } else {
        this.clearUserProfileData();
        //this.showUserModal = false;
        this.showProfileDropdown = false; // Fecha dropdown ao deslogar
        this.showProfileEditModal = false; // Fecha modal de edição ao deslogar
      }
    });
  }

  loadUserProfile(): void {
    if (!this.authService.getToken()) {
      this.clearUserProfileData();
      return;
    }
    this.authService.getUserProfile().subscribe({
      next: (profile: DonorReadDTO) => {
        this.currentUserProfile = profile;
        this.userName = profile.name;
        
        if (profile.profilePictureUrl) {
          this.userImageUrl = profile.profilePictureUrl;
          this.userInitials = null; 
        } else {
          this.userImageUrl = null;
          this.userInitials = this.getInitials(profile.name);
          this.avatarBackgroundColor = this.generateColorForName(profile.name);
        }
      },
      error: (err) => {
        console.error('Erro ao buscar perfil no header:', err.message);
        this.clearUserProfileData();
      }
    });
  }

  private clearUserProfileData(): void {
    this.userName = null;
    this.userImageUrl = 'assets/img/avatar-default.png';
    this.userInitials = null;
    this.avatarBackgroundColor = '#cccccc';
    this.currentUserProfile = null;
  }

  private getInitials(name: string): string {
    if (!name || name.trim() === '') {
      return '?'; 
    }
    const nameParts = name.trim().split(/\s+/);
    if (nameParts.length === 0) {
      return '?';
    }
    const firstInitial = nameParts[0][0] || '';
    const lastInitial = nameParts.length > 1 ? (nameParts[nameParts.length - 1][0] || '') : '';

    if (nameParts.length === 1 && firstInitial) {
        return nameParts[0].substring(0, Math.min(2, nameParts[0].length)).toUpperCase();
    }

    return (firstInitial + lastInitial).toUpperCase();
  }

  private generateColorForName(name: string): string {
    if (!name || name.trim() === '') {
      return this.avatarColors[0];
    }
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
      hash = hash & hash; 
    }
    const index = Math.abs(hash % this.avatarColors.length);
    return this.avatarColors[index];
  }

  ngOnDestroy(): void {
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
  }

//   toggleUserModal(): void {
//   if (this.isUserLoggedIn) {
//     if (!this.showUserModal) { 
//       console.log('HeaderComponent: Dados que serão passados para o modal (currentUserProfile):', JSON.parse(JSON.stringify(this.currentUserProfile)));
//       if (!this.currentUserProfile) {
//          this.loadUserProfile(); 
//       }
//     }
//     this.showUserModal = !this.showUserModal;
//   }
// }

      // NOVO: Alterna a visibilidade do dropdown
  toggleProfileDropdown(): void {
    this.showProfileDropdown = !this.showProfileDropdown;
    // Se o dropdown estiver abrindo e o modal de edição estiver aberto, feche o modal.
    if (this.showProfileDropdown && this.showProfileEditModal) {
      this.showProfileEditModal = false;
    }
  }


  // NOVO: Abre o modal de edição a partir do dropdown
  openEditProfileModal(): void {
    if (this.isUserLoggedIn && this.currentUserProfile) {
      this.showProfileEditModal = true;  // Mostra o modal de edição
      this.showProfileDropdown = false; // Esconde o dropdown
      console.log('Header: Abrindo modal de edição de perfil. currentUserProfile:', this.currentUserProfile);
    } else if (this.isUserLoggedIn && !this.currentUserProfile) {
        this.loadUserProfile(); // Tenta carregar perfil se não estiver pronto
        // O usuário talvez precise clicar de novo após o carregamento
        console.warn("Header: Tentou abrir modal de edição, mas currentUserProfile era nulo. Tentando carregar.");
    }
  }

  // NOVO: Fecha o modal de edição (chamado pelo @Output do ProfileModalComponent)
  closeEditProfileModal(): void {
    this.showProfileEditModal = false;
    console.log('Header: Modal de edição de perfil fechado.');
  }



  logout(): void {
    this.authService.logout();
    this.showProfileDropdown = false;    // Garante que o dropdown feche
    this.showProfileEditModal = false;   // Garante que o modal de edição feche
  }

  onProfileUpdated(): void {
    this.loadUserProfile();
    this.closeEditProfileModal(); // Fecha o modal após a atualização
    this.toastMessage = 'Perfil atualizado com sucesso!';
    this.toastVisible = true;
    setTimeout(() => { this.toastVisible = false; }, 3000);
  }
}