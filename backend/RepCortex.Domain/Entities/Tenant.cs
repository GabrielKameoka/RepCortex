using RepCortex.Domain.Entities.Enums;

namespace RepCortex.Domain.Entities;

/// <summary>
/// Representa um cliente/organização no sistema (Isolamento Multi-tenant).
/// </summary>
public class Tenant
{
    public string Id { get; private set; } // Slug único
    public string NomeComercial { get; private set; }
    public string PublishableKey { get; private set; }
    public string SecretKey { get; private set; }
    public string DominiosAutorizados { get; private set; }
    public bool Ativo { get; private set; }
    public PoliticaModeracao PoliticaModeracao { get; private set; }
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;

    private Tenant()
    {
        Id = string.Empty;
        NomeComercial = string.Empty;
        PublishableKey = string.Empty;
        SecretKey = string.Empty;
        DominiosAutorizados = string.Empty;
        PoliticaModeracao = PoliticaModeracao.Automatica;
    }

    public static Tenant Restaurar(string id, string nomeComercial, string publishableKey,
        string secretKey, string dominiosAutorizados, bool ativo,
        PoliticaModeracao politicaModeracao, DateTime dataCriacao)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(nomeComercial) ||
            string.IsNullOrWhiteSpace(publishableKey) || string.IsNullOrWhiteSpace(secretKey) ||
            string.IsNullOrWhiteSpace(dominiosAutorizados) || !Enum.IsDefined(politicaModeracao))
            throw new ArgumentException("Dados do tenant em cache inválidos.");

        return new Tenant
        {
            Id = id,
            NomeComercial = nomeComercial,
            PublishableKey = publishableKey,
            SecretKey = secretKey,
            DominiosAutorizados = dominiosAutorizados,
            Ativo = ativo,
            PoliticaModeracao = politicaModeracao,
            DataCriacao = dataCriacao
        };
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
        PoliticaModeracao = PoliticaModeracao.Automatica;
    }

    public void Desativar() => Ativo = false;
    public void Ativar() => Ativo = true;

    public void DefinirPoliticaModeracao(PoliticaModeracao politica)
    {
        PoliticaModeracao = politica;
    }
}
