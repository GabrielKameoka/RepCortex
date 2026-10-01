import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authService = jasmine.createSpyObj<AuthService>('AuthService', ['login', 'registrar']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    router.navigate.and.resolveTo(true);
    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router }
      ]
    });
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('shows the secret key once before entering the dashboard', () => {
    authService.registrar.and.returnValue(of({
      sucesso: true, mensagem: 'Criado', tenantId: 'minha-loja',
      tokenJWT: 'token', publishableKey: 'publica', secretKey: 'segredo-unico'
    }));
    component.toggleMode();
    component.authForm.setValue({
      tenantId: 'minha-loja', nomeComercial: 'Minha Loja', nomeCompleto: 'Mariana',
      email: 'mariana@loja.com', senha: 'senha1234'
    });
    component.onSubmit();
    fixture.detectChanges();

    expect(router.navigate).not.toHaveBeenCalled();
    expect((fixture.nativeElement as HTMLElement).querySelector<HTMLInputElement>('#secretKey')?.value).toBe('segredo-unico');

    component.entrarNoPainel();
    expect(component.registroConcluido()).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });
});
