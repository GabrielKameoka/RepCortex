using RepCortex.Domain.Entities;

namespace RepCortex.Domain.Interfaces.Repository;

public interface ITenantRepository
{
    Task AdicionarAsync(Tenant tenant);
    Task<bool> ExisteSlugAsync(string id);
    Task<Tenant?> ObterPorApiKeyAsync(string apiKey);
}