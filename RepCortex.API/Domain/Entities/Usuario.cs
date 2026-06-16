using RepCortex.Domain.Interfaces.Entities;

namespace RepCortex.Domain.Entities;

/// <summary>
/// Usuário da plataforma, pertence a um tenant específico.
/// </summary>
public class Usuario : ITenantEntity
{
    public string Id { get; private set; }

    public string NomeCompleto { get; private set; }

    public string Email { get; private set; }

    public string TenantId { get; private set; }

    public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;

    public virtual Tenant Tenant { get; private set; }

    private Usuario()
    {
    }

    public Usuario(string id, string nomeCompleto, string email, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id inválido.");
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new ArgumentException("O nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O e-mail é obrigatório.");
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("O usuário deve pertencer a um espaço (Tenant).");

        Id = id;
        NomeCompleto = nomeCompleto;
        Email = email;
        TenantId = tenantId;
    }
}