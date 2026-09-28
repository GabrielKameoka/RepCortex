using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Application.UseCases;

public class TenantService : ITenantService
{
    private string? _tenantId;

    public string ObterTenantId()
    {
        if (string.IsNullOrWhiteSpace(_tenantId))
            throw new UnauthorizedAccessException("Tenant não identificado na requisição.");

        return _tenantId;
    }

    public void DefinirTenantId(string tenantId) => _tenantId = tenantId;
}
