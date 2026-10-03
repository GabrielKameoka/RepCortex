using RepCortex.Application.DTOs.Dashboard;
using RepCortex.Application.Abstractions.Persistence;

namespace RepCortex.Application.Services;

public sealed class DashboardService(IAvaliacaoRepository repository, TimeProvider clock)
{
    public async Task<TenantDashboardMetrics> ObterMetricasAsync(string tenantId)
    {
        var hojeUtc = clock.GetUtcNow().UtcDateTime.Date;
        var inicioUtc = hojeUtc.AddDays(-6);
        var fimUtc = hojeUtc.AddDays(1);
        var resumo = await repository.ObterResumoAsync(tenantId);
        var volumetria = await repository.ObterVolumetriaAsync(tenantId, inicioUtc, fimUtc);
        var porDia = volumetria.ToDictionary(item => item.DiaUtc.Date, item => item.Quantidade);
        var pontos = Enumerable.Range(0, 7)
            .Select(offset => inicioUtc.AddDays(offset))
            .Select(dia => new GraficoLinhaPonto(dia.ToString("dd/MM"),
                porDia.GetValueOrDefault(dia)))
            .ToList();

        return new TenantDashboardMetrics(
            resumo.Total, Math.Round(resumo.MediaNotas, 1), resumo.Positivas,
            resumo.Neutras, resumo.Negativas, resumo.Pendentes, pontos);
    }
}
