import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardComponent } from './dashboard.component';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let dashboardService: any;

  beforeEach(() => {
    dashboardService = {
      metricas: signal(null), carregando: signal(false), erroMetricas: signal(null),
      atualizacaoRealtime: signal(0),
      obtenerMetricasIniciais: jasmine.createSpy('obtenerMetricasIniciais'),
      obterAvaliacoes: jasmine.createSpy('obterAvaliacoes').and.returnValue(of([
        { id: '1', produtoId: 'sku-1', usuarioIdExterno: 'autor-1', nomeUsuarioExterno: 'Mariana', nota: 5, comentario: 'Gostei', status: 'Pendente', sentimento: 'Positivo', dataCriacao: '2026-10-01T12:00:00Z' },
        { id: '2', produtoId: 'sku-2', usuarioIdExterno: 'autor-2', nomeUsuarioExterno: null, nota: 2, comentario: 'Ruim', status: 'Aprovada', sentimento: 'Negativo', dataCriacao: '2026-10-01T13:00:00Z' }
      ])),
      obterPoliticaModeracao: jasmine.createSpy('obterPoliticaModeracao').and.returnValue(of({ politica: 1 })),
      obterChavePublica: jasmine.createSpy('obterChavePublica').and.returnValue(of({ publishableKey: 'pk-teste' })),
      aprovarAvaliacao: jasmine.createSpy('aprovarAvaliacao').and.returnValue(of({})),
      iniciarConexaoRealtime: jasmine.createSpy('iniciarConexaoRealtime'),
      fecharConexao: jasmine.createSpy('fecharConexao')
    };
    TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        { provide: DashboardService, useValue: dashboardService },
        { provide: AuthService, useValue: { logout: jasmine.createSpy('logout') } },
        { provide: Router, useValue: { navigate: jasmine.createSpy('navigate') } }
      ]
    });
    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('opens on pending reviews and filters locally by product', () => {
    expect(component.abaAtiva()).toBe('fila');
    expect(component.pendentes()).toBe(1);
    expect(component.avaliacoesFiltradas().map(item => item.id)).toEqual(['1']);
    component.filtro.set('todas');
    component.busca.set('sku-2');
    fixture.detectChanges();
    expect(component.avaliacoesFiltradas().map(item => item.id)).toEqual(['2']);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Nome não informado');
  });

  it('refreshes reviews and metrics after approving a review', () => {
    component.aprovarComment('1');
    expect(dashboardService.aprovarAvaliacao).toHaveBeenCalledWith('1');
    expect(dashboardService.obterAvaliacoes).toHaveBeenCalledTimes(2);
    expect(dashboardService.obtenerMetricasIniciais).toHaveBeenCalledTimes(2);
    expect(component.mensagemAcao()).toBe('Avaliação aprovada.');
  });

  it('refreshes the review queue after a realtime event', () => {
    dashboardService.atualizacaoRealtime.set(1);
    fixture.detectChanges();
    expect(dashboardService.obterAvaliacoes).toHaveBeenCalledTimes(2);
  });
});
