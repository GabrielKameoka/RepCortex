import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environment/environment'; // Ajuste os caminhos relativos se necessário

export interface LoginResponse {
  token: string;
}

export interface RegistrarResponse {
  sucesso: boolean;
  mensagem: string;
  tenantId: string;
  tokenJWT: string;
  publishableKey: string;
  secretKey: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  
  public isAuthenticated = signal<boolean>(this.hasToken());

  constructor(private http: HttpClient) {}

  public login(tenantId: string, email: string, senha: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { tenantId, email, senha }).pipe(
      tap(res => this.definirSessao(res.token))
    );
  }

  public registrar(tenantIdSlug: string, nomeComercial: string, nomeCompletoUsuario: string, email: string, senha: string): Observable<RegistrarResponse> {
    return this.http.post<RegistrarResponse>(`${this.apiUrl}/registrar`, {
      tenantIdSlug,
      nomeComercial,
      nomeCompletoUsuario,
      email,
      senha
    }).pipe(
      tap(res => {
        if (res.sucesso && res.tokenJWT) {
          this.definirSessao(res.tokenJWT);
          if (res.publishableKey) {
            sessionStorage.setItem('repcortex_publishable_key', res.publishableKey);
          }
        }
      })
    );
  }

  public logout(): void {
    localStorage.removeItem('repcortex_token');
    sessionStorage.removeItem('repcortex_publishable_key');
    this.isAuthenticated.set(false);
  }

  public getToken(): string | null {
    return localStorage.getItem('repcortex_token');
  }

  public getPublishableKey(): string | null {
    return sessionStorage.getItem('repcortex_publishable_key');
  }

  public hasValidSession(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return typeof payload.exp !== 'number' || payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  private definirSessao(token: string): void {
    localStorage.setItem('repcortex_token', token);
    this.isAuthenticated.set(true);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem('repcortex_token');
  }
}
