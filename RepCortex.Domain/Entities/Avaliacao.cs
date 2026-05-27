namespace RepCortex.Domain.Entities;

public enum StatusAvaliacao
{
    Pendente,
    Aprovada,
    Rejeitada
}

public class Avaliacao
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ClienteId { get; private set; }
    public string UsuarioIdExterno { get; private set; }
    public string ProdutoId { get; private set; }
    public int Nota { get; private set; }
    public string Comentario { get; private set; }
    public string IpOrigem { get; private set; }
    public string Fingerprint { get; private set; }
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
    public StatusAvaliacao Status { get; private set; } = StatusAvaliacao.Pendente;
    public string Sentimento { get; private set; } = "Não analizado";

    public Avaliacao(string clienteId, string usuarioIdExterno, string produtoId, int nota, string comentario,      
        string ipOrigem, string fingerprint, string sentimento)
    {
        if (nota < 1 || nota > 5)
            throw new ArgumentException("A nota deve estar entre 1 e 5.");

        if (string.IsNullOrWhiteSpace(clienteId) || string.IsNullOrWhiteSpace(usuarioIdExterno))
            throw new ArgumentException("Identificadores inválidos.");

        ClienteId = clienteId;
        UsuarioIdExterno = usuarioIdExterno;
        ProdutoId = produtoId;
        Nota = nota;
        Comentario = comentario ?? string.Empty;
        IpOrigem = ipOrigem;
        Fingerprint = fingerprint;
        Sentimento = sentimento;

        // REGRA DE NEGÓCIO DA IA: 
        // Se a nota for boa (4 ou 5) mas o texto for negativo, o status fica retido como Pendente para moderação.
        if ((nota >= 4) && sentimento == "Negativo")
        {
            Status = StatusAvaliacao.Pendente;
        }
        // Caso contrário, se for nota 5 e sentimento positivo, pode até auto-aprovar no futuro.
        else
        {
            Status = StatusAvaliacao.Pendente;
        }
    }

    public void Aprovar() => Status = StatusAvaliacao.Aprovada;
    public void Rejeitar() => Status = StatusAvaliacao.Rejeitada;
}