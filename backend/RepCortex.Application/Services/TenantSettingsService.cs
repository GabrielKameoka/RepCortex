using RepCortex.Application.Abstractions.Persistence;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Entities.Enums;

namespace RepCortex.Application.Services;

public sealed class TenantSettingsService(ITenantRepository repository)
{
    public Task<Tenant?> ObterAsync(string tenantId) => repository.ObterPorIdAsync(tenantId);

    public async Task<Tenant?> DefinirPoliticaModeracaoAsync(string tenantId, PoliticaModeracao politica)
    {
        var tenant = await repository.ObterPorIdAsync(tenantId);
        if (tenant is null) return null;
        tenant.DefinirPoliticaModeracao(politica);
        await repository.AtualizarAsync(tenant);
        return tenant;
    }
}
