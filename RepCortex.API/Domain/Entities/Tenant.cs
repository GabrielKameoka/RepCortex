namespace RepCortex.Domain.Entities;

/// <summary>
/// Representa um cliente/organização no sistema (Isolamento Multi-tenant).
/// </summary>
public class Tenant
{
    public string Id { get; private set; } // Slug único
    public string NomeComercial { get; private set; }
    public string ApiKey { get; private set; }
    public string DominiosAutorizados { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;

    private Tenant() { }

    public Tenant(string id, string nomeComercial, string? dominiosAutorizados = null)
    {
        if (string.IsNullOrWhiteSpace(id)) 
            throw new ArgumentException("O identificador do Tenant é obrigatório.");
        if (string.IsNullOrWhiteSpace(nomeComercial)) 
            throw new ArgumentException("O nome comercial é obrigatório.");

        Id = id.ToLower().Trim().Replace(" ", "-"); 
        NomeComercial = nomeComercial;
        ApiKey = "rc_pub_" + Guid.NewGuid().ToString("N");
        DominiosAutorizados = dominiosAutorizados ?? "localhost";
        Ativo = true; 
    }

    public void Desativar() => Ativo = false;
    public void Ativar() => Ativo = true;
}