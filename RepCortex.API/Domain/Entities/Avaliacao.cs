using RepCortex.Domain.Entities.Enums;
using RepCortex.Domain.Interfaces.Entities;

namespace RepCortex.Domain.Entities;

/// <summary>
/// Representa uma avaliação de cliente com lógica de moderação automática baseada em sentimento.
/// </summary>
public class Avaliacao : ITenantEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ClienteId { get; private set; }
    public string UsuarioIdExterno { get; private set; }
    public string? NomeUsuarioExterno { get; private set; }
    public string ProdutoId { get; private set; }
    public int Nota { get; private set; }
    public string Comentario { get; private set; }
    public string IpOrigem { get; private set; }
    public string Fingerprint { get; private set; }
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
    public StatusAvaliacao Status { get; private set; } = StatusAvaliacao.Pendente;
    public SentimentoAvaliacao Sentimento { get; private set; } = SentimentoAvaliacao.NaoAnalisado;
    public string? Resposta { get; private set; }
    public string TenantId { get; private set; }
    public virtual Tenant Tenant { get; private set; } = null!;

    // O EF Core usa este construtor ao materializar avaliações existentes.
    // A política de moderação é aplicada somente na criação de uma nova avaliação.
    private Avaliacao()
    {
        TenantId = string.Empty;
        ClienteId = string.Empty;
        UsuarioIdExterno = string.Empty;
        ProdutoId = string.Empty;
        Comentario = string.Empty;
        IpOrigem = string.Empty;
        Fingerprint = string.Empty;
    }

    /// <summary>
    /// Construtor principal que executa validações de negócio e define o status inicial via IA.
    /// </summary>
    public Avaliacao(string tenantId, string clienteId, string usuarioIdExterno, string produtoId, int nota,
        string comentario, string ipOrigem, string fingerprint, SentimentoAvaliacao sentimento,
        PoliticaModeracao politicaModeracao = PoliticaModeracao.Automatica, string? nomeUsuarioExterno = null)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("O TenantId é obrigatório.");

        if (nota < 1 || nota > 5)
            throw new ArgumentException("A nota deve estar entre 1 e 5.");

        if (string.IsNullOrWhiteSpace(clienteId) || string.IsNullOrWhiteSpace(usuarioIdExterno))
            throw new ArgumentException("Identificadores inválidos.");

        if (string.IsNullOrWhiteSpace(produtoId))
            throw new ArgumentException("O produto é obrigatório.");

        if (string.IsNullOrWhiteSpace(comentario))
            throw new ArgumentException("O comentário é obrigatório.");

        var nomeNormalizado = nomeUsuarioExterno?.Trim();
        if (nomeNormalizado?.Length > 100)
            throw new ArgumentException("O nome do usuário externo deve ter até 100 caracteres.");

        TenantId = tenantId;
        ClienteId = clienteId;
        UsuarioIdExterno = usuarioIdExterno;
        NomeUsuarioExterno = string.IsNullOrEmpty(nomeNormalizado) ? null : nomeNormalizado;
        ProdutoId = produtoId;
        Nota = nota;
        Comentario = comentario ?? string.Empty;
        IpOrigem = ipOrigem;
        Fingerprint = fingerprint;
        Sentimento = sentimento;

        Status = DefinirStatusInicial(nota, sentimento, politicaModeracao);
    }

    /// <summary>
    /// Regras de moderação automática: Nota 5 + Positivo aprova direto; demais casos retêm para revisão.
    /// </summary>
    private StatusAvaliacao DefinirStatusInicial(
        int nota,
        SentimentoAvaliacao sentimento,
        PoliticaModeracao politicaModeracao)
    {
        if (politicaModeracao == PoliticaModeracao.Manual)
            return StatusAvaliacao.Pendente;

        if (nota == 5 && sentimento == SentimentoAvaliacao.Positivo)
            return StatusAvaliacao.Aprovada;

        if (nota >= 4 && sentimento == SentimentoAvaliacao.Negativo)
            return StatusAvaliacao.Pendente;

        if (nota <= 2)
            return StatusAvaliacao.Pendente;

        return StatusAvaliacao.Pendente;
    }

    public void Aprovar() => Status = StatusAvaliacao.Aprovada;

    public void Rejeitar() => Status = StatusAvaliacao.Rejeitada;

    public void Responder(string resposta)
    {
        if (string.IsNullOrWhiteSpace(resposta))
            throw new ArgumentException("A resposta não pode ser vazia.");

        Resposta = resposta;

        if (Status == StatusAvaliacao.Pendente)
        {
            Aprovar();
        }
    }
}
