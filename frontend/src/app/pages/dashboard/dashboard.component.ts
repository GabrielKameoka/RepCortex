import { Component, OnInit, OnDestroy, ViewChild, ElementRef, computed, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import Chart from 'chart.js/auto';
import { AvaliacaoDashboard, DashboardService, MetricasDashboard } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environment/environment';

type Aba = 'fila' | 'analise' | 'integracao';
type Filtro = 'pendentes' | 'todas' | 'aprovadas' | 'rejeitadas';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css', './dashboard-panels.css']
})
export class DashboardComponent implements OnInit, OnDestroy {
  @ViewChild('chartCanvas') chartCanvas?: ElementRef<HTMLCanvasElement>;
  private chart?: Chart;

  public abaAtiva = signal<Aba>('fila');
  public filtro = signal<Filtro>('pendentes');
  public busca = signal('');
  public avaliacoes = signal<AvaliacaoDashboard[]>([]);
  public pendentes = computed(() => this.avaliacoes().filter(item => this.estado(item) === 'pendentes').length);
  public avaliacoesFiltradas = computed(() => {
    const termo = this.busca().trim().toLocaleLowerCase('pt-BR');
    return [...this.avaliacoes()]
      .filter(item => this.filtro() === 'todas' || this.estado(item) === this.filtro())
      .filter(item => !termo || [item.produtoId, item.nomeUsuarioExterno, item.usuarioIdExterno, item.comentario]
        .some(valor => valor?.toLocaleLowerCase('pt-BR').includes(termo)))
      .sort((a, b) => Number(this.estado(b) === 'pendentes') - Number(this.estado(a) === 'pendentes') ||
        Date.parse(b.dataCriacao) - Date.parse(a.dataCriacao));
  });
  public carregandoComentarios = signal(false);
  public erroComentarios = signal<string | null>(null);
  public mensagemAcao = signal<string | null>(null);
  public acaoEmAndamento = signal<string | null>(null);
  public politicaModeracao = signal<number | null>(null);
  public salvandoPolitica = signal(false);
  public erroPolitica = signal<string | null>(null);
  public mensagemPolitica = signal<string | null>(null);
  public chavePublica = signal<string | null>(null);
  public erroChavePublica = signal<string | null>(null);
  public mensagemCopia = signal<string | null>(null);
  public enviandoTeste = signal(false);
  public mensagemTeste = signal<string | null>(null);
  public erroTeste = signal<string | null>(null);
  public readonly apiBaseUrl = environment.apiUrl;
  public readonly exemploPayload = JSON.stringify({
    usuarioIdExterno: 'cliente-123',
    nomeUsuarioExterno: 'Mariana',
    produtoId: 'produto-123',
    nota: 5,
    comentario: 'Produto chegou como esperado.',
    fingerprint: 'identificador-do-dispositivo'
  }, null, 2);

  constructor(
    public dashboardService: DashboardService,
    public authService: AuthService,
    private router: Router
  ) {
    effect(() => {
      const dados = this.dashboardService.metricas();
      if (dados && this.abaAtiva() === 'analise') {
        setTimeout(() => this.atualizarGrafico(dados.volumetriaUltimosDias), 0);
      }
    });
    effect(() => {
      if (this.dashboardService.atualizacaoRealtime() > 0) {
        this.carregarComentarios();
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    this.dashboardService.obtenerMetricasIniciais();
    this.carregarComentarios();
    this.carregarPoliticaModeracao();
    this.carregarChavePublica();
    this.dashboardService.iniciarConexaoRealtime();
  }

  ngOnDestroy(): void {
    this.dashboardService.fecharConexao();
    this.chart?.destroy();
  }

  public selecionarAba(aba: Aba): void {
    this.abaAtiva.set(aba);
  }

  public estado(item: AvaliacaoDashboard): Filtro {
    const status = item.status.toLocaleLowerCase('pt-BR');
    if (status.startsWith('aprovad')) return 'aprovadas';
    if (status.startsWith('rejeitad')) return 'rejeitadas';
    return 'pendentes';
  }

  public sentimento(item: AvaliacaoDashboard): string {
    const valor = String(item.sentimento).toLocaleLowerCase('pt-BR');
    if (valor === '1' || valor === 'positivo') return 'Positivo';
    if (valor === '2' || valor === 'neutro') return 'Neutro';
    if (valor === '3' || valor === 'negativo') return 'Negativo';
    return 'Não classificado';
  }

  public classeSentimento(item: AvaliacaoDashboard): string {
    return this.sentimento(item).toLocaleLowerCase('pt-BR').replace('não classificado', 'sem-classificacao');
  }

  public carregarComentarios(): void {
    this.carregandoComentarios.set(true);
    this.erroComentarios.set(null);
    this.dashboardService.obterAvaliacoes().subscribe({
      next: dados => {
        this.avaliacoes.set(dados);
        this.carregandoComentarios.set(false);
      },
      error: erro => {
        this.erroComentarios.set(this.mensagemErro(erro, 'Não foi possível carregar as avaliações.'));
        this.carregandoComentarios.set(false);
      }
    });
  }

  public aprovarComment(id: string): void {
    this.executarAcao(id, 'Avaliação aprovada.', this.dashboardService.aprovarAvaliacao(id));
  }

  public rejeitarComment(id: string): void {
    this.executarAcao(id, 'Avaliação rejeitada.', this.dashboardService.rejeitarAvaliacao(id));
  }

  public responderComment(id: string, campo: HTMLTextAreaElement): void {
    const resposta = campo.value.trim();
    if (!resposta) {
      this.erroComentarios.set('Escreva uma resposta antes de publicar.');
      return;
    }
    this.executarAcao(id, 'Resposta publicada.', this.dashboardService.responderAvaliacao(id, resposta), campo);
  }

  private executarAcao(id: string, mensagem: string, requisicao: Observable<unknown>, campo?: HTMLTextAreaElement): void {
    if (this.acaoEmAndamento()) return;
    this.acaoEmAndamento.set(id);
    this.erroComentarios.set(null);
    this.mensagemAcao.set(null);
    requisicao.subscribe({
      next: () => {
        if (campo) campo.value = '';
        this.acaoEmAndamento.set(null);
        this.mensagemAcao.set(mensagem);
        this.carregarComentarios();
        this.dashboardService.obtenerMetricasIniciais();
      },
      error: erro => {
        this.acaoEmAndamento.set(null);
        this.erroComentarios.set(this.mensagemErro(erro, 'Não foi possível concluir a ação. Tente novamente.'));
      }
    });
  }

  public carregarPoliticaModeracao(): void {
    this.dashboardService.obterPoliticaModeracao().subscribe({
      next: resposta => this.politicaModeracao.set(resposta.politica),
      error: erro => this.erroPolitica.set(this.mensagemErro(erro, 'Não foi possível carregar a política.'))
    });
  }

  public alterarPoliticaModeracao(event: Event): void {
    const campo = event.target as HTMLSelectElement;
    const politica = Number(campo.value);
    if (![1, 2].includes(politica)) return;
    this.erroPolitica.set(null);
    this.mensagemPolitica.set(null);
    this.salvandoPolitica.set(true);
    this.dashboardService.atualizarPoliticaModeracao(politica).subscribe({
      next: resposta => {
        this.politicaModeracao.set(resposta.politica);
        this.mensagemPolitica.set('Política de publicação atualizada.');
        this.salvandoPolitica.set(false);
      },
      error: erro => {
        this.erroPolitica.set(this.mensagemErro(erro, 'Não foi possível salvar a política.'));
        this.salvandoPolitica.set(false);
        campo.value = String(this.politicaModeracao());
      }
    });
  }

  public carregarChavePublica(): void {
    this.erroChavePublica.set(null);
    this.dashboardService.obterChavePublica().subscribe({
      next: resposta => this.chavePublica.set(resposta.publishableKey),
      error: erro => this.erroChavePublica.set(this.mensagemErro(erro, 'Não foi possível carregar a chave pública.'))
    });
  }

  public async copiarChavePublica(): Promise<void> {
    const chave = this.chavePublica();
    if (!chave) return;
    try {
      await navigator.clipboard.writeText(chave);
      this.mensagemCopia.set('Chave pública copiada.');
    } catch {
      this.mensagemCopia.set('Não foi possível copiar. Selecione a chave e copie manualmente.');
    }
  }

  public enviarAvaliacaoTeste(notaTexto: string, comentario: string, nomeUsuarioExterno: string, campoComentario: HTMLTextAreaElement): void {
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
        this.mensagemTeste.set(`Avaliação criada com status ${resposta.status}. Veja o resultado na Fila.`);
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
      ? resposta.mensagem : padrao;
  }

  private atualizarGrafico(volumetria: MetricasDashboard['volumetriaUltimosDias']): void {
    const contexto = this.chartCanvas?.nativeElement.getContext('2d');
    if (!contexto) return;
    const labels = volumetria.map(item => item.data);
    const valores = volumetria.map(item => item.quantidade);
    if (this.chart) {
      this.chart.data.labels = labels;
      this.chart.data.datasets[0].data = valores;
      this.chart.update();
      return;
    }
    this.chart = new Chart(contexto, {
      type: 'line',
      data: { labels, datasets: [{ data: valores, borderColor: '#b793f8', backgroundColor: 'rgba(183,147,248,.13)', fill: true, tension: .3, pointRadius: 4 }] },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, grid: { color: '#3e374b' }, ticks: { color: '#b8afca', precision: 0 } },
          x: { grid: { display: false }, ticks: { color: '#b8afca' } }
        }
      }
    });
  }

  public executarLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
