import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  public authForm: FormGroup;
  public isLoginMode = signal<boolean>(true); // Alterna entre Login e Cadastro
  public errorMessage = signal<string>('');
  public isLoading = signal<boolean>(false);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.authForm = this.fb.group({
      tenantId: ['', [Validators.required]],
      nomeComercial: [''],
      nomeCompleto: [''],
      email: ['', [Validators.required, Validators.email]],
      senha: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  public toggleMode(): void {
    this.isLoginMode.set(!this.isLoginMode());
    this.errorMessage.set('');
    this.authForm.reset();
  }

  public onSubmit(): void {
    if (this.authForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');
    
    const { tenantId, nomeComercial, nomeCompleto, email, senha } = this.authForm.value;

    if (this.isLoginMode()) {
      this.authService.login(tenantId, email, senha).subscribe({
        next: () => this.router.navigate(['/dashboard']),
        error: (err) => {
          this.errorMessage.set(err.error?.mensagem || 'Falha ao realizar login.');
          this.isLoading.set(false);
        }
      });
    } else {
      this.authService.registrar(tenantId, nomeComercial, nomeCompleto, email, senha).subscribe({
        next: (res) => {
          if (res.sucesso) {
            this.router.navigate(['/dashboard']);
          } else {
            this.errorMessage.set(res.mensagem);
            this.isLoading.set(false);
          }
        },
        error: (err) => {
          this.errorMessage.set(err.error?.mensagem || 'Erro ao registrar espaço.');
          this.isLoading.set(false);
        }
      });
    }
  }
}