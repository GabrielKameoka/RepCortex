import { Injectable, signal, WritableSignal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environment/environment'; // Ajuste os caminhos relativos se necessário

export interface MetricasDashboard {
  totalAvaliacoes: number;
  mediaNotas: number;
  totalPositivas: number;
  totalNeutras: number;
  totalNegativas: number;
  totalPendentesModeracao: number;
  volumetriaUltimosDias: { data: string; quantidade: number }[];
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

  constructor(private http: HttpClient) {}

  public obterAvaliacoes() {
    const token = localStorage.getItem('repcortex_token');
    return this.http.get<any[]>(this.avaliacoesUrl, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  public aprovarAvaliacao(id: string) {
    const token = localStorage.getItem('repcortex_token');
    return this.http.post(`${this.avaliacoesUrl}/${id}/aprovar`, {}, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  public rejeitarAvaliacao(id: string) {
    const token = localStorage.getItem('repcortex_token');
    return this.http.post(`${this.avaliacoesUrl}/${id}/rejeitar`, {}, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  public responderAvaliacao(id: string, resposta: string) {
    const token = localStorage.getItem('repcortex_token');
    return this.http.post(`${this.avaliacoesUrl}/${id}/responder`, { resposta }, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  public obtenerMetricasIniciais(): void {
    this.carregando.set(true);
    const token = localStorage.getItem('repcortex_token');
    
    this.http.get<MetricasDashboard>(this.apiUrl, {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
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
    const token = localStorage.getItem('repcortex_token');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token ? token : ''
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
}