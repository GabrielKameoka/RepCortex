import { Injectable, signal, WritableSignal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environment/environment'; // Ajuste os caminhos relativos se necessário
import { AuthService } from './auth.service';

export interface MetricasDashboard {
  totalAvaliacoes: number;
  mediaNotas: number;
  totalPositivas: number;
  totalNeutras: number;
  totalNegativas: number;
  totalPendentesModeracao: number;
  volumetriaUltimosDias: { data: string; quantidade: number }[];
}

export interface AvaliacaoDashboard {
  id: string;
  produtoId: string;
  clienteId: string;
  usuarioIdExterno: string;
  nomeUsuarioExterno: string | null;
  nota: number;
  comentario: string;
  status: string;
  sentimento: number | string;
  dataCriacao: string;
  resposta?: string;
}

export interface PoliticaModeracaoResponse {
  politica: number;
}

export interface AvaliacaoTesteRequest {
  clienteId: string;
  usuarioIdExterno: string;
  nomeUsuarioExterno?: string;
  produtoId: string;
  nota: number;
  comentario: string;
  fingerprint: string;
}

export interface AvaliacaoTesteResponse {
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/admin/dashboard/metricas`;
  private hubUrl = `${environment.hubUrl}/dashboard`;
  private avaliacoesUrl = `${environment.apiUrl}/admin/avaliacoes`;
  
  private hubConnection?: signalR.HubConnection;
  
  public metricas: WritableSignal<MetricasDashboard | null> = signal<MetricasDashboard | null>(null);
  public carregando = signal<boolean>(false);

  constructor(private http: HttpClient, private authService: AuthService) {}

  public obterAvaliacoes() {
    return this.http.get<AvaliacaoDashboard[]>(this.avaliacoesUrl);
  }

  public aprovarAvaliacao(id: string) {
    return this.http.post(`${this.avaliacoesUrl}/${id}/aprovar`, {});
  }

  public rejeitarAvaliacao(id: string) {
    return this.http.post(`${this.avaliacoesUrl}/${id}/rejeitar`, {});
  }

  public responderAvaliacao(id: string, resposta: string) {
    return this.http.post(`${this.avaliacoesUrl}/${id}/responder`, { resposta });
  }

  public obtenerMetricasIniciais(): void {
    this.carregando.set(true);
    this.http.get<MetricasDashboard>(this.apiUrl).subscribe({
      next: (dados) => {
        this.metricas.set(dados);
        this.carregando.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar métricas via HTTP:', err);
        this.carregando.set(false);
      }
    });
  }

  public iniciarConexaoRealtime(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('WebSocket conectado ao barramento de eventos do RepCortex.'))
      .catch(err => console.error('Falha na conexão em tempo real:', err));

    this.hubConnection.on('ReceberMetricasAtualizadas', (novasMetricas: MetricasDashboard) => {
      this.metricas.set(novasMetricas);
    });
  }

  public fecharConexao(): void {
    this.hubConnection?.stop();
  }

  public obterPoliticaModeracao() {
    return this.http.get<PoliticaModeracaoResponse>(`${environment.apiUrl}/admin/configuracoes/moderacao`);
  }

  public atualizarPoliticaModeracao(politica: number) {
    return this.http.put<PoliticaModeracaoResponse>(`${environment.apiUrl}/admin/configuracoes/moderacao`, { politica });
  }

  public obterChavePublica() {
    return this.http.get<{ publishableKey: string }>(`${environment.apiUrl}/admin/integracao/chave-publica`);
  }

  public enviarAvaliacaoTeste(chavePublica: string, avaliacao: AvaliacaoTesteRequest) {
    return this.http.post<AvaliacaoTesteResponse>(`${environment.apiUrl}/public/avaliacoes`, avaliacao, {
      headers: { 'x-api-key': chavePublica }
    });
  }
}
