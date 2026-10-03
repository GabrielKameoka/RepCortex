using Microsoft.AspNetCore.SignalR;
using RepCortex.Application.Abstractions.Services;
using RepCortex.Application.Services;

namespace RepCortex.API.Hubs;

public sealed class DashboardEventPublisher(
    IHubContext<DashboardHub> hub,
    DashboardService dashboard,
    ILogger<DashboardEventPublisher> logger) : IDashboardEventPublisher
{
    public async Task PublicarAtualizacaoAsync(string tenantId)
    {
        try
        {
            var metricas = await dashboard.ObterMetricasAsync(tenantId);
            await hub.Clients.Group(tenantId)
                .SendAsync("ReceberMetricasAtualizadas", metricas);
        }
        catch (Exception erro)
        {
            // A avaliação já foi persistida. Uma falha no canal de atualização
            // não deve transformar a resposta HTTP em erro ou induzir reenvio.
            logger.LogError(erro, "Falha ao atualizar dashboard do tenant {TenantId}", tenantId);
        }
    }
}
