import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { CommonModule } from '@angular/common'; 
import { AuthService } from '../core/services/auth.service';
import { RegisterDonorDto } from '../core/models/register-donor.dto'; 

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule
  ],
  templateUrl: './cadastro.component.html',
  styleUrls: ['./cadastro.component.scss'] 
})
export class CadastroComponent implements OnInit {

  registrationForm!: FormGroup; 
  isLoading = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  showPassword = false;
  showConfirmPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.registrationForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(150)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      password: ['', [Validators.required, Validators.minLength(8)]], 
      confirmPassword: ['', Validators.required]
    }, {
      validators: this.passwordMatchValidator 
    });
  }

  get f() { return this.registrationForm.controls; }

  onSubmit(): void {
    this.successMessage = null;
    this.errorMessage = null;
    this.isLoading = true;

    this.registrationForm.markAllAsTouched();

    if (this.registrationForm.invalid) {
      this.isLoading = false;
      this.errorMessage = "Por favor, corrija os erros no formulário.";
      return;
    }

    
    const donorData: RegisterDonorDto = {
      FullName: this.registrationForm.value.fullName, 
      Email: this.registrationForm.value.email,
      Password: this.registrationForm.value.password,
      ConfirmPassword: this.registrationForm.value.confirmPassword
    };

    this.authService.register(donorData).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.successMessage = response.message || 'Cadastro realizado com sucesso!';
        this.registrationForm.reset();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Ocorreu um erro ao tentar realizar o cadastro.';
        console.error('Erro no registro:', err);
      }
    });
  }

  

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    // Se os controles ainda não existem, ou se confirmPassword não foi tocado/preenchido ainda, não valide
    if (!password || !confirmPassword || !confirmPassword.dirty) {
      return null;
    }

    if (password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ mismatch: true }); // Define o erro no controle confirmPassword
      return { mismatch: true }; // Define o erro no FormGroup
    } else {
      // Se as senhas batem, mas o controle confirmPassword AINDA TEM o erro 'mismatch'
      // de uma validação anterior, precisamos limpá-lo.
      if (confirmPassword.errors && confirmPassword.errors['mismatch']) {
        confirmPassword.setErrors(null);
      }
      return null;
    }
  }
}