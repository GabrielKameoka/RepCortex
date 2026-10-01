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
  public isLoginMode = signal<boolean>(true);
  public errorMessage = signal<string>('');
  public isLoading = signal<boolean>(false);
  public registroConcluido = signal<{ tenantId: string; secretKey: string; publishableKey: string } | null>(null);
  public mensagemCopia = signal<string>('');

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
    this.registroConcluido.set(null);
    this.authForm.reset();

    for (const field of ['nomeComercial', 'nomeCompleto']) {
      const control = this.authForm.get(field);
      control?.setValidators(this.isLoginMode() ? [] : [Validators.required, Validators.pattern(/\S/)]);
      control?.updateValueAndValidity();
    }
  }

  public onSubmit(): void {
    if (this.authForm.invalid) {
      this.authForm.markAllAsTouched();
      return;
    }

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
            this.registroConcluido.set({
              tenantId: res.tenantId,
              secretKey: res.secretKey,
              publishableKey: res.publishableKey
            });
            this.isLoading.set(false);
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

  public async copiarChaveSecreta(): Promise<void> {
    const chave = this.registroConcluido()?.secretKey;
    if (!chave) return;

    try {
      await navigator.clipboard.writeText(chave);
      this.mensagemCopia.set('Chave copiada. Guarde-a em um local seguro.');
    } catch {
      this.mensagemCopia.set('Não foi possível copiar automaticamente. Selecione a chave e copie manualmente.');
    }
  }

  public entrarNoPainel(): void {
    this.registroConcluido.set(null);
    this.router.navigate(['/dashboard']);
  }

  private mensagemErro(err: HttpErrorResponse, padrao: string): string {
    if (err.status === 0) return 'Não foi possível conectar à API. Tente novamente.';
    if (typeof err.error?.mensagem === 'string') return err.error.mensagem;
    if (err.error?.errors) return 'Verifique os campos obrigatórios e tente novamente.';
    return padrao;
  }
}
