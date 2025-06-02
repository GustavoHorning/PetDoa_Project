import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {

  isDashboard = false;
  showUserModal = false;
  userForm: FormGroup;
  userImageUrl = 'assets/img/avatar-default.png';

  // ✅ Toast
  toastVisible = false;
  toastMessage = '';

  constructor(private router: Router, private fb: FormBuilder) {
    this.userForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.router.events.subscribe(() => {
      this.isDashboard = this.router.url.includes('dashboard');
    });
  }

  toggleUserModal(): void {
    this.showUserModal = !this.showUserModal;
  }

  handleFileInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const reader = new FileReader();
      reader.onload = () => {
        this.userImageUrl = reader.result as string;
      };
      reader.readAsDataURL(input.files[0]);
    }
  }

  submitUserData(): void {
    const { email, password } = this.userForm.value;

    
    this.toastMessage = 'Dados do usuário atualizados com sucesso!';
    this.toastVisible = true;

    
    this.showUserModal = false;
    this.userForm.reset();
    this.userImageUrl = 'assets/img/avatar-default.png';

    
    setTimeout(() => {
      this.toastVisible = false;
    }, 3000);
  }
}
