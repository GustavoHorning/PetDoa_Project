import { Component, EventEmitter, Output, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { DonorReadDTO, UpdateDonorProfileDto, ChangePasswordDto } from '../../../core/models/donor.dtos'; 

@Component({
  selector: 'app-profile-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], 
  templateUrl: './profile-modal.component.html',
  styleUrls: ['./profile-modal.component.scss'] 
})
export class ProfileModalComponent implements OnInit, OnChanges {
  @Input() profileData!: DonorReadDTO;
  @Output() closeModal = new EventEmitter<void>();
  @Output() profileUpdated = new EventEmitter<void>(); 

  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  displayImageUrl: string | null = null; 
  displayUserInitials: string | null = null;
  displayAvatarBackgroundColor: string = '#cccccc'; 
  imagePreviewForUpload: string | ArrayBuffer | null = null;
  selectedFile: File | null = null;
  isLoadingName = false;
  isLoadingPassword = false;
  isLoadingPicture = false;

  message: { type?: 'success' | 'error' | 'info', text?: string } = {};

  private readonly avatarColors = [
    '#F44336', '#E91E63', '#9C27B0', '#673AB7', '#3F51B5',
    '#2196F3', '#03A9F4', '#00BCD4', '#009688', '#4CAF50',
    '#8BC34A', '#CDDC39', '#FFC107', '#FF9800', '#FF5722',
    '#795548', '#607D8B', '#4DB6AC', '#AED581', '#FFB74D', '#A1887F'
  ];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService
  ) {
  }

  ngOnInit(): void {
    console.log('MODAL ngOnInit: Invocado.');
    console.log('MODAL ngOnInit: profileData ANTES de initForms:', this.profileData ? JSON.parse(JSON.stringify(this.profileData)) : 'undefined/null');
    this.initForms();
    if (this.profileData) {
      console.log('MODAL ngOnInit: profileData existe, chamando populateFormsAndHandleAvatar.');
      this.populateFormsAndHandleAvatar(this.profileData);
    } else {
      console.warn('MODAL ngOnInit: profileData NÃO existe. Configurando avatar para default/iniciais.');
      this.displayImageUrl = null;
      this.displayUserInitials = this.getInitials(''); // Ou '?'
      this.displayAvatarBackgroundColor = this.avatarColors[0];
      this.imagePreviewForUpload = null;
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    console.log('MODAL ngOnChanges: Invocado. Mudanças detectadas:', changes);
    if (changes['profileData'] && changes['profileData'].currentValue) {
      const newProfileData = changes['profileData'].currentValue as DonorReadDTO;
      console.log('MODAL ngOnChanges: profileData recebido/atualizado:', JSON.parse(JSON.stringify(newProfileData)));
      if (!this.profileForm) {
        console.log('MODAL ngOnChanges: profileForm não inicializado, chamando initForms().');
        this.initForms();
      }
      this.populateFormsAndHandleAvatar(newProfileData);
    } else if (changes['profileData'] && !changes['profileData'].currentValue) {
        console.warn('MODAL ngOnChanges: profileData mudou para nulo/indefinido.');
        this.profileForm?.reset(); this.passwordForm?.reset();
        this.displayImageUrl = null; this.displayUserInitials = this.getInitials('');
        this.displayAvatarBackgroundColor = this.avatarColors[0];
        this.imagePreviewForUpload = null; this.selectedFile = null;
    }
  }


  initForms(): void {
    console.log('MODAL initForms: Criando formulários.');
    this.profileForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(150)]],
      email: [{ value: '', disabled: true }, [Validators.required, Validators.email]]
    });
    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required, Validators.minLength(8)]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmNewPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator.bind(this) });
  }

  populateFormsAndHandleAvatar(data: DonorReadDTO): void {
    console.log('MODAL populateFormsAndHandleAvatar: Recebeu dados:', data ? JSON.parse(JSON.stringify(data)) : 'undefined/null');
    if (data) {
      this.profileForm.patchValue({
        fullName: data.name,
        email: data.email
      });

      if (data.profilePictureUrl) {
        this.displayImageUrl = data.profilePictureUrl;
        this.displayUserInitials = null; 
        console.log('MODAL populateFormsAndHandleAvatar: displayImageUrl definido como:', this.displayImageUrl);
      } else {
        this.displayImageUrl = null;
        this.displayUserInitials = this.getInitials(data.name);
        this.displayAvatarBackgroundColor = this.generateColorForName(data.name);
        console.log('MODAL populateFormsAndHandleAvatar: displayUserInitials definido como:', this.displayUserInitials, 'com cor:', this.displayAvatarBackgroundColor);
      }
    } else {
      console.warn('MODAL populateFormsAndHandleAvatar: Dados nulos. Configurando avatar para default.');
      this.displayImageUrl = null;
      this.displayUserInitials = this.getInitials('');
      this.displayAvatarBackgroundColor = this.avatarColors[0];
    }
    this.imagePreviewForUpload = null;
    this.selectedFile = null;
    console.log('MODAL populateFormsAndHandleAvatar: imagePreviewForUpload e selectedFile resetados.');
    console.log('MODAL populateForms FINAL: displayImageUrl:', this.displayImageUrl, 
                '| displayUserInitials:', this.displayUserInitials, 
                '| bgColor:', this.displayAvatarBackgroundColor,
                '| imagePreviewForUpload:', this.imagePreviewForUpload);

    this.passwordForm.reset();
    this.message = {};
  }

  private getInitials(name: string): string {
    if (!name || name.trim() === '') { console.log("MODAL GET_INITIALS: Nome vazio, retornando '?'"); return '?'; }
    const nameParts = name.trim().split(/\s+/);
    if (nameParts.length === 0) { console.log("MODAL GET_INITIALS: Nenhuma parte no nome, retornando '?'"); return '?'; }
    const firstInitial = nameParts[0][0] || '';
    const lastInitial = nameParts.length > 1 ? (nameParts[nameParts.length - 1][0] || '') : '';
    let initialsResult;
    if (nameParts.length === 1 && firstInitial) {
        initialsResult = nameParts[0].substring(0, Math.min(2, nameParts[0].length)).toUpperCase();
    } else {
        initialsResult = (firstInitial + lastInitial).toUpperCase();
    }
    console.log(`MODAL GET_INITIALS: Para nome '${name}', gerou '${initialsResult}'`);
    return initialsResult;
  }

  private generateColorForName(name: string): string {
    if (!name || name.trim() === '') { return this.avatarColors[0]; }
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
      hash = hash & hash;
    }
    const index = Math.abs(hash % this.avatarColors.length);
    console.log(`MODAL GENERATE_COLOR: Para nome '${name}', cor: ${this.avatarColors[index]}`);
    return this.avatarColors[index];
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword');
    const confirmNewPassword = control.get('confirmNewPassword');

    if (!newPassword || !confirmNewPassword) {
      return null;
    }
    if (confirmNewPassword.dirty || confirmNewPassword.touched) {
      if (newPassword.value !== confirmNewPassword.value) {
        confirmNewPassword.setErrors({ mismatch: true });
        return { mismatch: true };
      } else {
        if (confirmNewPassword.errors && confirmNewPassword.errors['mismatch']) {
          delete confirmNewPassword.errors['mismatch'];
          if (Object.keys(confirmNewPassword.errors).length === 0) {
            confirmNewPassword.setErrors(null);
          } else {
             confirmNewPassword.setErrors(confirmNewPassword.errors);
          }
        }
      }
    }
    return null;
  }

  onClose(): void {
    this.closeModal.emit();
  }

  onUpdateName(): void {
    this.profileForm.get('fullName')?.markAsTouched();
    if (this.profileForm.get('fullName')?.invalid) {
      this.showMessage('error', 'Nome inválido. Verifique o campo.');
      return;
    }
    this.isLoadingName = true; this.message = {};
    const updateDto: UpdateDonorProfileDto = { Name: this.profileForm.value.fullName };

    this.authService.updateProfileName(updateDto).subscribe({
      next: () => {
        this.isLoadingName = false;
        this.showMessage('success', 'Nome atualizado com sucesso!');
        this.profileUpdated.emit(); 
      },
      error: (err) => {
        this.isLoadingName = false;
        this.showMessage('error', err.message || 'Falha ao atualizar o nome.');
      }
    });
  }

  onChangePassword(): void {
    this.passwordForm.markAllAsTouched();
    if (this.passwordForm.invalid) {
      this.showMessage('error', 'Dados de senha inválidos. Verifique os campos e se as senhas coincidem.');
      return;
    }
    this.isLoadingPassword = true; this.message = {};
    const passwordDto: ChangePasswordDto = this.passwordForm.value;

    this.authService.changePassword(passwordDto).subscribe({
      next: (response: any) => {
        this.isLoadingPassword = false;
        this.showMessage('success', response.message || 'Senha alterada com sucesso!');
        this.passwordForm.reset();
      },
      error: (err) => {
        this.isLoadingPassword = false;
        this.showMessage('error', err.message || 'Falha ao alterar a senha.');
      }
    });
  }

  onFileSelected(event: Event): void {
    console.log('MODAL onFileSelected: Evento disparado.');
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedFile = input.files[0];
      console.log('MODAL onFileSelected: Arquivo selecionado:', this.selectedFile.name);
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviewForUpload = reader.result;
        console.log('MODAL onFileSelected: imagePreviewForUpload definido.');
      };
      reader.readAsDataURL(this.selectedFile);
      this.message = {};
      input.value = '';
    } else {
      this.selectedFile = null;
      this.imagePreviewForUpload = null;
      console.log('MODAL onFileSelected: Seleção de arquivo cancelada, imagePreviewForUpload limpo.');
    }
  }

  onUploadPicture(): void {
    console.log('MODAL onUploadPicture: Chamado. SelectedFile:', this.selectedFile?.name);
    if (!this.selectedFile) { this.showMessage('error', 'Nenhuma nova imagem selecionada.'); return; }
    this.isLoadingPicture = true; this.message = {};
    this.authService.uploadProfilePicture(this.selectedFile).subscribe({
      next: (response) => {
        console.log('MODAL onUploadPicture: Sucesso na API. Resposta:', response);
        this.isLoadingPicture = false;
        this.showMessage('success', 'Foto de perfil atualizada!');
        this.selectedFile = null;
        this.imagePreviewForUpload = null; 
        console.log('MODAL onUploadPicture: Emitindo profileUpdated.');
        this.profileUpdated.emit();
      },
      error: (err) => {  }
    });
  }

  onDeletePicture(): void {
    console.log('MODAL onDeletePicture: Chamado. profileData?.profilePictureUrl atual:', this.profileData?.profilePictureUrl);
    if (!this.profileData?.profilePictureUrl) { 
        console.log('MODAL onDeletePicture: Nenhuma foto personalizada no profileData para remover.');
        this.showMessage('info', 'Nenhuma foto de perfil para remover.');
        return;
    }
    this.isLoadingPicture = true; this.message = {};
    this.authService.deleteProfilePicture().subscribe({
      next: () => {
        console.log('MODAL onDeletePicture: Sucesso na API.');
        this.isLoadingPicture = false;
        this.showMessage('success', 'Foto de perfil removida.');
        this.selectedFile = null;
        this.imagePreviewForUpload = null; 
        console.log('MODAL onDeletePicture: Emitindo profileUpdated.');
        this.profileUpdated.emit(); 
      },
      error: (err) => { /* ... */ }
    });
  }

  showMessage(type: 'success' | 'error' | 'info', text: string, duration: number = 4000) {
    this.message = { type, text };
    setTimeout(() => { this.message = {}; }, duration);
  }
}