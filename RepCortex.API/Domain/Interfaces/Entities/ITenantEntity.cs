namespace RepCortex.Domain.Interfaces.Entities;

/// <summary>
/// Marca uma entidade como multi-tenant (deve ter propriedade TenantId).
/// Query filters globais garantem isolamento de dados.
/// </summary>
public interface ITenantEntity
{
    /// <summary>Identificador do tenant proprietário desta entidade.</summary>
    string TenantId { get; }
}