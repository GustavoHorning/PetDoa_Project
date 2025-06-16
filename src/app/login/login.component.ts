import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms'; 
import { Router, RouterModule } from '@angular/router'; 
import { AuthService } from '../core/services/auth.service';
import { LoginDonorDto } from '../core/models/login-donor.dto';
import { AdminLoginDto } from '../core/models/admin.dtos.ts'; 


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
  isLoginAdmin = false;


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

  toggleLoginMode(): void {
    this.isLoginAdmin = !this.isLoginAdmin;
    this.errorMessage = null;
    this.loginForm.reset();
  }

  // onSubmitEmailPassword(): void {
  //   this.errorMessage = null; 
  //   this.isLoading = true;
  //   this.loginForm.markAllAsTouched();

  //   if (this.loginForm.invalid) {
  //     this.isLoading = false;
  //     this.errorMessage = "Por favor, preencha todos os campos corretamente.";
  //     return;
  //   }

  //   const credentials: LoginDonorDto = {
  //     Email: this.loginForm.value.email,
  //     Password: this.loginForm.value.password
  //   };

  //   this.authService.login(credentials).subscribe({
  //     next: (response) => {
  //       this.isLoading = false;
  //       console.log('Login bem-sucedido, token:', response.token);
  //       this.router.navigate(['/dashboard']); 
  //     },
  //     error: (err) => {
  //       this.isLoading = false;
  //       this.errorMessage = err.message || 'Falha no login. Verifique suas credenciais.';
  //       console.error('Erro no login:', err);
  //     }
  //   });
  // }

  // onSubmit(): void {
  //   if (this.loginForm.invalid) {
  //     this.errorMessage = "Por favor, preencha todos os campos.";
  //     return;
  //   }
  //   this.isLoading = true;
  //   this.errorMessage = null;

  //   if (this.isLoginAdmin) {
  //     // Se for login de admin, chama o método de admin
  //     const credentials: AdminLoginDTO = this.loginForm.value;
  //     this.authService.loginAdmin(credentials).subscribe(this.handleLoginResponse.bind(this));
  //   } else {
  //     // Se for login de doador, chama o método de doador
  //     const credentials: LoginDonorDto = this.loginForm.value;
  //     this.authService.loginDonor(credentials).subscribe(this.handleLoginResponse.bind(this));
  //   }
  // }

  // onSubmit(): void {
  //   if (this.loginForm.invalid) {
  //     this.errorMessage = "Por favor, preencha todos os campos.";
  //     return;
  //   }
  //   this.isLoading = true;
  //   this.errorMessage = null;

  //   const credentials = this.loginForm.value;
  //   const loginObservable = this.isLoginAdmin 
  //     ? this.authService.loginAdmin(credentials as AdminLoginDto)
  //     : this.authService.loginDonor(credentials as LoginDonorDto);

  //   loginObservable.subscribe(this.handleLoginResponse);
  // }

//   onSubmit(): void {
//   if (this.loginForm.invalid) {
//     this.errorMessage = "Por favor, preencha todos os campos.";
//     return;
//   }
//   this.isLoading = true;
//   this.errorMessage = null;

//   const credentials = this.loginForm.value;
//   const loginObservable = this.isLoginAdmin 
//     ? this.authService.loginAdmin(credentials as AdminLoginDto)
//     : this.authService.loginDonor(credentials as LoginDonorDto);

//   loginObservable.subscribe({
//     next: (response) => this.handleLoginResponse(response),
//     error: (err) => {
//       this.isLoading = false;
//       this.errorMessage = err.message || 'Falha no login. Verifique suas credenciais.';
//     }
//   });
// }

onSubmit(): void {
    if (this.loginForm.invalid) {
      this.errorMessage = "Por favor, preencha todos os campos.";
      return;
    }
    this.isLoading = true;
    this.errorMessage = null;

    const credentials = this.loginForm.value;
    
    // Escolhe qual método do serviço chamar com base no modo
    const loginObservable = this.isLoginAdmin 
      ? this.authService.loginAdmin(credentials as AdminLoginDto)
      : this.authService.loginDonor(credentials as LoginDonorDto);

    loginObservable.subscribe({
      next: (response) => {
        this.isLoading = false;
        
        // A resposta do AuthService já deve incluir a 'role'
        // E o AuthService já salvou a sessão
        const userRole = this.authService.getUserRole();

        // Redirecionamento com base na role salva
        if (userRole === 'Admin' || userRole === 'SuperAdmin') {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Falha no login. Verifique suas credenciais.';
      }
    });
  }

  // private handleLoginResponse = {
  //   next: (response: { token: string; role: string; }) => {
  //     this.isLoading = false;
      
  //     // O AuthService deve salvar o token e a role
  //     // this.authService.saveSession(response.token, response.role);

  //     if (response.role === 'Admin' || response.role === 'SuperAdmin') {
  //       this.router.navigate(['/admin/dashboard']);
  //     } else {
  //       this.router.navigate(['/dashboard']);
  //     }
  //   },
  //   error: (err: any) => {
  //     this.isLoading = false;
  //     this.errorMessage = err.message || 'Falha no login. Verifique suas credenciais.';
  //   }
  // };

  private handleLoginResponse(response: { token: string; role: string }): void {
  this.isLoading = false;

  if (response.role === 'Admin' || response.role === 'SuperAdmin') {
    this.router.navigate(['/admin/dashboard']);
  } else {
    this.router.navigate(['/dashboard']);
  }
}

  signInWithGoogle(): void {
    this.isLoading = true; 
    this.authService.initiateGoogleAuth();
  }
}