using RepCortex.Application.DTOs.Dashboard;
using RepCortex.Application.Abstractions.Persistence;
using RepCortex.Domain.Entities.Enums;

namespace RepCortex.Application.Services;

public sealed class DashboardService(IAvaliacaoRepository repository)
{
    public async Task<TenantDashboardMetrics> ObterMetricasAsync(string tenantId)
    {
        var lista = (await repository.ObterTodosAsync(tenantId)).ToList();
        return new TenantDashboardMetrics(
            TotalAvaliacoes: lista.Count,
            MediaNotas: lista.Count > 0 ? Math.Round(lista.Average(a => a.Nota), 1) : 0,
            TotalPositivas: lista.Count(a => a.Sentimento == SentimentoAvaliacao.Positivo),
            TotalNeutras: lista.Count(a => a.Sentimento == SentimentoAvaliacao.Neutro),
            TotalNegativas: lista.Count(a => a.Sentimento == SentimentoAvaliacao.Negativo),
            TotalPendentesModeracao: lista.Count(a => a.Status == StatusAvaliacao.Pendente),
            VolumetriaUltimosDias: lista
                .GroupBy(a => a.DataCriacao.ToString("dd/MM"))
                .OrderBy(g => g.Key)
                .Take(7)
                .Select(g => new GraficoLinhaPonto(g.Key, g.Count()))
                .ToList());
    }
}
