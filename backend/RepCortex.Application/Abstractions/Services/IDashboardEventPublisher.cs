namespace RepCortex.Application.Abstractions.Services;

public interface IDashboardEventPublisher
{
    Task PublicarAtualizacaoAsync(string tenantId);
}
