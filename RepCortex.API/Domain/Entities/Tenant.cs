using System.Text.Json.Serialization;

namespace RepCortex.Domain.Entities;

/// <summary>
/// Representa um cliente/organização no sistema (Isolamento Multi-tenant).
/// </summary>
public class Tenant
{
    [JsonInclude]
    public string Id { get; private set; } // Slug único
    [JsonInclude]
    public string NomeComercial { get; private set; }
    [JsonInclude]
    public string PublishableKey { get; private set; }
    [JsonInclude]
    public string SecretKey { get; private set; }
    [JsonInclude]
    public string DominiosAutorizados { get; private set; }
    [JsonInclude]
    public bool Ativo { get; private set; }
    [JsonInclude]
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;

    [JsonConstructor]
    private Tenant()
    {
        Id = string.Empty;
        NomeComercial = string.Empty;
        PublishableKey = string.Empty;
        SecretKey = string.Empty;
        DominiosAutorizados = string.Empty;
    }

    public Tenant(string id, string nomeComercial, string? dominiosAutorizados = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("O identificador do Tenant é obrigatório.");
        if (string.IsNullOrWhiteSpace(nomeComercial))
            throw new ArgumentException("O nome comercial é obrigatório.");

        Id = id.ToLower().Trim().Replace(" ", "-");
        NomeComercial = nomeComercial;
        PublishableKey = "rc_pub_" + Guid.NewGuid().ToString("N");
        SecretKey = "rc_sec_" + Guid.NewGuid().ToString("N");
        DominiosAutorizados = dominiosAutorizados ?? "localhost";
        Ativo = true;
    }

    public void Desativar() => Ativo = false;
    public void Ativar() => Ativo = true;
}