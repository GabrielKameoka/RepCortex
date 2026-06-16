using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Application.UseCases;

public class TenantService : ITenantService
{
    private string? _tenantId;

    public string ObterTenantId()
    {
        if (string.IsNullOrWhiteSpace(_tenantId))
        {
            // Retorna vazio em vez de estourar erro se nenhum tenant foi definido ainda.
            // Isso permite o Onboarding rodar livremente!
            return string.Empty; 
        }

        return _tenantId;
    }

    public void DefinirTenantId(string tenantId) => _tenantId = tenantId;
}