import { Component, OnInit, OnDestroy, ViewChild, ElementRef, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environment/environment';
import { Router } from '@angular/router';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {
  @ViewChild('chartCanvas') chartCanvas!: ElementRef;
  private chart?: Chart;

  // Estado reativo para controlar a navegação interna do Dashboard
  public abaAtiva = signal<string>('metricas');
  public avaliacoes = signal<any[]>([]);
  public carregandoComentarios = signal<boolean>(false);

  constructor(
    public dashboardService: DashboardService,
    private authService: AuthService,
    private router: Router,
    private http: HttpClient
  ) {
    effect(() => {
      const dados = this.dashboardService.metricas();
      const aba = this.abaAtiva();

      // Executa em um microtask para garantir que o ciclo de renderização do Angular terminou
      // e o canvas realmente exista no DOM se a aba mudou.
      setTimeout(() => {
        if (dados && aba === 'metricas' && this.chartCanvas) {
          this.atualizarGrafico(dados.volumetriaUltimosDias);
        }
      }, 0);
    });
  }

  public carregarComentarios(): void {
    this.carregandoComentarios.set(true);
    this.dashboardService.obterAvaliacoes().subscribe({
      next: (dados) => {
        this.avaliacoes.set(dados);
        this.carregandoComentarios.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar comentários:', err);
        this.carregandoComentarios.set(false);
      }
    });
  }

  public aprovarComment(id: string): void {
    this.dashboardService.aprovarAvaliacao(id).subscribe({
      next: () => {
        this.carregarComentarios();
        this.dashboardService.obtenerMetricasIniciais();
      },
      error: (err) => console.error('Erro ao aprovar comentário:', err)
    });
  }

  public rejeitarComment(id: string): void {
    this.dashboardService.rejeitarAvaliacao(id).subscribe({
      next: () => {
        this.carregarComentarios();
        this.dashboardService.obtenerMetricasIniciais();
      },
      error: (err) => console.error('Erro ao rejeitar comentário:', err)
    });
  }

  public responderComment(id: string, resposta: string): void {
    if (!resposta.trim()) return;
    this.dashboardService.responderAvaliacao(id, resposta).subscribe({
      next: () => {
        this.carregarComentarios();
        this.dashboardService.obtenerMetricasIniciais();
      },
      error: (err) => console.error('Erro ao responder comentário:', err)
    });
  }

  ngOnInit(): void {
    this.dashboardService.obtenerMetricasIniciais();
    this.dashboardService.iniciarConexaoRealtime();
  }

  ngOnDestroy(): void {
    this.dashboardService.fecharConexao();
    if (this.chart) {
      this.chart.destroy();
    }
  }

  public enviarAvaliacaoTeste(nota: string, comentario: string): void {
    if (!comentario.trim()) return;

    const tenantIdDoAdmin = this.obterTenantIdLogado(); 
  
  const idAleatorio = Math.floor(Math.random() * 1000000);

  const payload = {
    tenantId: tenantIdDoAdmin,
    clienteId: `cli_sandbox_${idAleatorio}`,
    usuarioIdExterno: `usr_sandbox_${idAleatorio}`,
    produtoId: `prod_simulado_${idAleatorio}`, 
    nota: Number(nota),
    comentario: comentario,
    ipOrigem: '127.0.0.1',
    fingerprint: `sandbox_fingerprint_${idAleatorio}`
  };

    const publishableKey = 'rc_pub_809cc0f890694489a19fc72ffee99f36';

    const urlBase = environment.apiUrl.endsWith('/api') ? environment.apiUrl : `${environment.apiUrl}/api`;

    this.http.post(`${urlBase}/public/avaliacoes`, payload, {
      headers: {
        'x-api-key': publishableKey,
        'Content-Type': 'application/json'
      }
    }).subscribe({
      next: () => {
        alert('Avaliação simulada com sucesso! O barramento SignalR deve atualizar a tela em instantes.');
      },
      error: (err) => {
        console.error('Erro ao simular avaliação no Sandbox:', err);
        // Exibe a mensagem exata tratada pela Exception da API caso ocorra outra
        const msgErro = err.error || 'Falha ao enviar simulação.';
        alert(`Erro: ${msgErro}`);
      }
    });
  }

  private atualizarGrafico(volumetria: any[]): void {
    if (!this.chartCanvas) return;

    const ctx = this.chartCanvas.nativeElement.getContext('2d');

    // Mapeia aceitando português, inglês, camelCase e PascalCase
    const labels = volumetria.map((v: any) => v.data || v.Data);

    const valores = volumetria.map((v: any) => {
      if (v.quantidade !== undefined) return v.quantidade;
      if (v.Quantidade !== undefined) return v.Quantidade;
      if (v.quantity !== undefined) return v.quantity;
      if (v.Quantity !== undefined) return v.Quantity;
      return 0; // fallback seguro para não quebrar o Chart.js
    });

    if (this.chart) {
      this.chart.data.labels = labels;
      this.chart.data.datasets[0].data = valores;
      this.chart.update();
    } else {
      this.chart = new Chart(ctx, {
        type: 'line',
        data: {
          labels: labels,
          datasets: [{
            label: 'Avaliações Recebidas',
            data: valores,
            borderColor: '#7c3aed',
            backgroundColor: 'rgba(124, 58, 237, 0.1)',
            tension: 0.3,
            fill: true
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { display: false } },
          scales: {
            y: { grid: { color: '#1f2937' }, ticks: { color: '#9ca3af' } },
            x: { grid: { display: false }, ticks: { color: '#9ca3af' } }
          }
        }
      });
    }
  }

  public obterTenantIdLogado(): string {
    const token = localStorage.getItem('repcortex_token');
    if (!token) return 'seu-tenant-slug';

    try {
      // Quebra as seções do JWT (Header.Payload.Signature) e decodifica a base64 do Payload
      const payloadBase64 = token.split('.')[1];
      const payloadDecodificado = JSON.parse(atob(payloadBase64));

      // Retorna a claim configurada na criação do Token no .NET
      return payloadDecodificado.tenant_id || 'seu-tenant-slug';
    } catch (e) {
      console.error('Falha ao parsear credencial de inquilino:', e);
      return 'seu-tenant-slug';
    }
  }

  public executarLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}