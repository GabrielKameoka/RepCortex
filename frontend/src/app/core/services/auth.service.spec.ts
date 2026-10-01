import { TestBed } from '@angular/core/testing';

import { AuthService } from './auth.service';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { environment } from '../../../environment/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [HttpClientTestingModule] });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.removeItem('repcortex_token');
    sessionStorage.removeItem('repcortex_publishable_key');
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('keeps the secret key out of browser storage after registration', () => {
    const secret = 'secret-only-in-response';
    service.registrar('minha-loja', 'Minha Loja', 'Mariana', 'm@loja.com', 'senha1234').subscribe();
    httpMock.expectOne(`${environment.apiUrl}/auth/registrar`).flush({
      sucesso: true, mensagem: 'Criado', tenantId: 'minha-loja',
      tokenJWT: 'jwt-teste', publishableKey: 'publica-teste', secretKey: secret
    });
    expect(localStorage.getItem('repcortex_token')).toBe('jwt-teste');
    expect(sessionStorage.getItem('repcortex_publishable_key')).toBe('publica-teste');
    expect(JSON.stringify(localStorage) + JSON.stringify(sessionStorage)).not.toContain(secret);
  });
});
