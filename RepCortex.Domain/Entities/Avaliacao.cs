using RepCortex.Domain.Entities.Enums;
using RepCortex.Domain.Interfaces.Entities;

namespace RepCortex.Domain.Entities;

public class Avaliacao : ITenantEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ClienteId { get; private set; }
    public string TenantId { get; private set; }
    public string UsuarioIdExterno { get; private set; }
    public string ProdutoId { get; private set; }
    public int Nota { get; private set; }
    public string Comentario { get; private set; }
    public string IpOrigem { get; private set; }
    public string Fingerprint { get; private set; }
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
    public StatusAvaliacao Status { get; private set; } = StatusAvaliacao.Pendente;
    public SentimentoAvaliacao Sentimento { get; private set; } = SentimentoAvaliacao.NaoAnalisado;

    public Avaliacao(string tenantId, string clienteId, string usuarioIdExterno, string produtoId, int nota,
        string comentario, string ipOrigem, string fingerprint, SentimentoAvaliacao sentimento)
    {
        // 1. Validações de Consistência (Guards)
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("O TenantId é obrigatório.");

        if (nota < 1 || nota > 5)
            throw new ArgumentException("A nota deve estar entre 1 e 5.");

        if (string.IsNullOrWhiteSpace(clienteId) || string.IsNullOrWhiteSpace(usuarioIdExterno))
            throw new ArgumentException("Identificadores inválidos.");

        TenantId = tenantId;
        ClienteId = clienteId;
        UsuarioIdExterno = usuarioIdExterno;
        ProdutoId = produtoId;
        Nota = nota;
        Comentario = comentario ?? string.Empty;
        IpOrigem = ipOrigem;
        Fingerprint = fingerprint;
        Sentimento = sentimento;

        // 2. Executa a Esteira de Moderação Automática baseada nas regras de IA
        Status = DefinirStatusInicial(nota, sentimento);
    }
    
    // Encapsula as Regras de Negócio de forma isolada e legível
    private StatusAvaliacao DefinirStatusInicial(int nota, SentimentoAvaliacao sentimento)
    {
        // Nota máxima com feedback explicitamente positivo é auto-aprovada
        if (nota == 5 && sentimento == SentimentoAvaliacao.Positivo)
            return StatusAvaliacao.Aprovada;

        // Nota alta com texto negativo indica possível ironia ou reclamação oculta (Retém para moderação)
        if (nota >= 4 && sentimento == SentimentoAvaliacao.Negativo)
            return StatusAvaliacao.Pendente;

        // Notas baixas (críticas) sempre entram como pendentes para o lojista analisar o feedback do produto
        if (nota <= 2)
            return StatusAvaliacao.Pendente;

        // Qualquer outro cenário cai na regra geral de segurança
        return StatusAvaliacao.Pendente;
    }

    public void Aprovar() => Status = StatusAvaliacao.Aprovada;
    public void Rejeitar() => Status = StatusAvaliacao.Rejeitada;
}