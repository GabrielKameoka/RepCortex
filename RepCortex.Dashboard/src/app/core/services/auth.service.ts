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
  token: string;
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
        if (res.sucesso && res.token) {
          this.definirSessao(res.token);
        }
      })
    );
  }

  public logout(): void {
    localStorage.removeItem('repcortex_token');
    this.isAuthenticated.set(false);
  }

  private definirSessao(token: string): void {
    localStorage.setItem('repcortex_token', token);
    this.isAuthenticated.set(true);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem('repcortex_token');
  }
}