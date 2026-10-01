import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
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
      senha: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  public toggleMode(): void {
    this.isLoginMode.set(!this.isLoginMode());
    this.errorMessage.set('');
    this.authForm.reset();

    for (const field of ['nomeComercial', 'nomeCompleto']) {
      const control = this.authForm.get(field);
      control?.setValidators(this.isLoginMode() ? [] : [Validators.required, Validators.pattern(/\S/)]);
      control?.updateValueAndValidity();
    }
  }

  public onSubmit(): void {
    if (this.authForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');
    
    const { tenantId, nomeComercial, nomeCompleto, email, senha } = this.authForm.value;

    if (this.isLoginMode()) {
      this.authService.login(tenantId, email, senha).subscribe({
        next: () => this.router.navigate(['/dashboard']),
        error: (err: HttpErrorResponse) => {
          this.errorMessage.set(this.mensagemErro(err, 'Falha ao realizar login.'));
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
        error: (err: HttpErrorResponse) => {
          this.errorMessage.set(this.mensagemErro(err, 'Erro ao registrar espaço.'));
          this.isLoading.set(false);
        }
      });
    }
  }

  private mensagemErro(err: HttpErrorResponse, padrao: string): string {
    if (err.status === 0) return 'Não foi possível conectar à API. Tente novamente.';
    if (typeof err.error?.mensagem === 'string') return err.error.mensagem;
    if (err.error?.errors) return 'Verifique os campos obrigatórios e tente novamente.';
    return padrao;
  }
}
