import { Component, OnInit, OnDestroy, ViewChild, ElementRef, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
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
  public politicaModeracao = signal<number | null>(null);
  public salvandoPolitica = signal<boolean>(false);

  constructor(
    public dashboardService: DashboardService,
    public authService: AuthService,
    private router: Router
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
    this.carregarPoliticaModeracao();
    this.dashboardService.iniciarConexaoRealtime();
  }

  ngOnDestroy(): void {
    this.dashboardService.fecharConexao();
    if (this.chart) {
      this.chart.destroy();
    }
  }

  public carregarPoliticaModeracao(): void {
    this.dashboardService.obterPoliticaModeracao().subscribe({
      next: resposta => this.politicaModeracao.set(resposta.politica),
      error: erro => console.error('Erro ao carregar política de moderação:', erro)
    });
  }

  public alterarPoliticaModeracao(event: Event): void {
    const politica = Number((event.target as HTMLSelectElement).value);
    if (![1, 2].includes(politica)) return;

    this.salvandoPolitica.set(true);
    this.dashboardService.atualizarPoliticaModeracao(politica).subscribe({
      next: resposta => {
        this.politicaModeracao.set(resposta.politica);
        this.salvandoPolitica.set(false);
      },
      error: erro => {
        console.error('Erro ao atualizar política de moderação:', erro);
        this.salvandoPolitica.set(false);
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

  public executarLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
