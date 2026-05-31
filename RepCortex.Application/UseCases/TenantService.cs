using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Application.UseCases;

public class TenantService : ITenantService
{
    private string? _tenantId;

    public void DefinirTenantId(string tenantId) => _tenantId = tenantId;
    public string ObterTenantId() => _tenantId ?? throw new UnauthorizedAccessException("Tenant não identificado.");
}

