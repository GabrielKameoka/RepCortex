using RepCortex.Application.Abstractions.Persistence;
using RepCortex.Application.DTOs.Dashboard;
using RepCortex.Application.Services;
using RepCortex.Domain.Entities;
using Xunit;

namespace RepCortex.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task VolumetriaUsaSeteDiasEmOrdemCronologicaNaViradaDoAno()
    {
        var repositorio = new RepositorioMetricas();
        var relogio = new RelogioFixo(new DateTimeOffset(2026, 1, 2, 12, 0, 0, TimeSpan.Zero));
        var servico = new DashboardService(repositorio, relogio);

        var metricas = await servico.ObterMetricasAsync("loja-a");

        Assert.Equal("loja-a", repositorio.TenantConsultado);
        Assert.Equal(new DateTime(2025, 12, 27, 0, 0, 0, DateTimeKind.Utc), repositorio.Inicio);
        Assert.Equal(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc), repositorio.Fim);
        Assert.Equal(["27/12", "28/12", "29/12", "30/12", "31/12", "01/01", "02/01"],
            metricas.VolumetriaUltimosDias.Select(ponto => ponto.Data));
        Assert.Equal([0, 0, 0, 0, 2, 0, 1],
            metricas.VolumetriaUltimosDias.Select(ponto => ponto.Quantidade));
    }

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }

    private sealed class RepositorioMetricas : IAvaliacaoRepository
    {
        public string? TenantConsultado { get; private set; }
        public DateTime Inicio { get; private set; }
        public DateTime Fim { get; private set; }

        public Task<ResumoAvaliacoes> ObterResumoAsync(string tenantId)
        {
            TenantConsultado = tenantId;
            return Task.FromResult(new ResumoAvaliacoes(3, 4, 2, 0, 1, 1));
        }

        public Task<IReadOnlyList<VolumetriaDia>> ObterVolumetriaAsync(
            string tenantId, DateTime inicioUtc, DateTime fimUtc)
        {
            TenantConsultado = tenantId;
            Inicio = inicioUtc;
            Fim = fimUtc;
            IReadOnlyList<VolumetriaDia> dias = [
                new(new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc), 2),
                new(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), 1)
            ];
            return Task.FromResult(dias);
        }

        public Task AdicionarAsync(Avaliacao avaliacao) => throw new NotSupportedException();
        public Task<IEnumerable<Avaliacao>> ObterTodosAsync(string tenantId) => throw new NotSupportedException();
        public Task<(IReadOnlyList<Avaliacao> Itens, int Total)> ObterPublicadasAsync(
            string tenantId, string produtoId, int pagina, int tamanhoPagina) => throw new NotSupportedException();
        public Task<Avaliacao?> ObterPorIdAsync(Guid id) => throw new NotSupportedException();
        public Task AtualizarAsync(Avaliacao avaliacao) => throw new NotSupportedException();
        public Task<bool> JaAvaliouProdutoAsync(string produtoId, string fingerprint, string tenantId) =>
            throw new NotSupportedException();
    }
}
