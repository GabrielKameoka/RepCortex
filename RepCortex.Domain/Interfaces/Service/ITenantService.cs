namespace RepCortex.Domain.Interfaces.Service;

public interface ITenantService
{
    public void DefinirTenantId(string tenantId);
    string ObterTenantId();
}