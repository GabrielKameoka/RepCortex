import { Component, OnInit, OnDestroy, ViewChild, ElementRef, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AvaliacaoDashboard, DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
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
  public avaliacoes = signal<AvaliacaoDashboard[]>([]);
  public carregandoComentarios = signal<boolean>(false);
  public politicaModeracao = signal<number | null>(null);
  public salvandoPolitica = signal<boolean>(false);
  public erroPolitica = signal<string | null>(null);
  public chavePublica = signal<string | null>(null);
  public enviandoTeste = signal<boolean>(false);
  public mensagemTeste = signal<string | null>(null);
  public erroTeste = signal<string | null>(null);

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
    this.carregarChavePublica();
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
      error: erro => this.erroPolitica.set(this.mensagemErro(erro, 'Não foi possível carregar a política.'))
    });
  }

  public alterarPoliticaModeracao(event: Event): void {
    const politica = Number((event.target as HTMLSelectElement).value);
    if (![1, 2].includes(politica)) return;

    this.erroPolitica.set(null);
    this.salvandoPolitica.set(true);
    this.dashboardService.atualizarPoliticaModeracao(politica).subscribe({
      next: resposta => {
        this.politicaModeracao.set(resposta.politica);
        this.salvandoPolitica.set(false);
      },
      error: erro => {
        this.erroPolitica.set(this.mensagemErro(erro, 'Não foi possível salvar a política.'));
        this.salvandoPolitica.set(false);
        (event.target as HTMLSelectElement).value = String(this.politicaModeracao());
      }
    });
  }

  public carregarChavePublica(): void {
    this.dashboardService.obterChavePublica().subscribe({
      next: resposta => this.chavePublica.set(resposta.publishableKey),
      error: erro => this.erroTeste.set(this.mensagemErro(erro, 'Não foi possível carregar a chave pública.'))
    });
  }

  public enviarAvaliacaoTeste(
    notaTexto: string,
    comentario: string,
    nomeUsuarioExterno: string,
    campoComentario: HTMLTextAreaElement
  ): void {
    const nota = Number(notaTexto);
    this.mensagemTeste.set(null);
    this.erroTeste.set(null);

    if (!Number.isInteger(nota) || nota < 1 || nota > 5 || !comentario.trim()) {
      this.erroTeste.set('Informe uma nota de 1 a 5 e um comentário.');
      return;
    }
    if (nomeUsuarioExterno.trim().length > 100) {
      this.erroTeste.set('O nome do autor deve ter até 100 caracteres.');
      return;
    }

    const id = crypto.randomUUID();
    this.enviandoTeste.set(true);
    this.dashboardService.enviarAvaliacaoTeste({
      usuarioIdExterno: `usr_teste_${id}`,
      nomeUsuarioExterno: nomeUsuarioExterno.trim() || undefined,
      produtoId: 'produto-teste',
      nota,
      comentario: comentario.trim(),
      fingerprint: `teste_${id}`
    }).subscribe({
      next: resposta => {
        this.enviandoTeste.set(false);
        this.mensagemTeste.set(`Avaliação enviada com status ${resposta.status}. Confira na aba Comentários.`);
        campoComentario.value = '';
        this.carregarComentarios();
        this.dashboardService.obtenerMetricasIniciais();
      },
      error: erro => {
        this.enviandoTeste.set(false);
        this.erroTeste.set(this.mensagemErro(erro, 'Não foi possível enviar a avaliação.'));
      }
    });
  }

  private mensagemErro(erro: HttpErrorResponse, padrao: string): string {
    const resposta = erro.error;
    return typeof resposta === 'object' && resposta !== null && typeof resposta.mensagem === 'string'
      ? resposta.mensagem
      : padrao;
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
