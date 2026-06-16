namespace RepCortex.Domain.Interfaces.Service;

/// <summary>
/// Gerencia o contexto de tenant da requisição atual (scoped).
/// Define qual tenant está logado e permite recuperar o ID em qualquer camada.
/// </summary>
public interface ITenantService
{
    /// <summary>Define qual tenant está fazendo a requisição (chamado pelo TenantMiddleware).</summary>
    void DefinirTenantId(string tenantId);

    /// <summary>Obtém o ID do tenant logado. Lança UnauthorizedAccessException se não definido.</summary>
    string ObterTenantId();
}