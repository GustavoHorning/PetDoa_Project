import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms'; 
import { Router, RouterModule } from '@angular/router'; 
import { AuthService } from '../core/services/auth.service';
import { LoginDonorDto } from '../core/models/login-donor.dto';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router 
  ) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  get f() { return this.loginForm.controls; }

  onSubmitEmailPassword(): void {
    this.errorMessage = null; 
    this.isLoading = true;
    this.loginForm.markAllAsTouched();

    if (this.loginForm.invalid) {
      this.isLoading = false;
      this.errorMessage = "Por favor, preencha todos os campos corretamente.";
      return;
    }

    const credentials: LoginDonorDto = {
      Email: this.loginForm.value.email,
      Password: this.loginForm.value.password
    };

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.isLoading = false;
        console.log('Login bem-sucedido, token:', response.token);
        this.router.navigate(['/dashboard']); 
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Falha no login. Verifique suas credenciais.';
        console.error('Erro no login:', err);
      }
    });
  }

  signInWithGoogle(): void {
    this.isLoading = true; 
    this.authService.initiateGoogleAuth();
  }
}