namespace RepCortex.Application.DTOs;

public class CriarAvaliacaoRequest
{
    public string ClienteId { get; set; } = string.Empty;
    public string UsuarioIdExterno { get; set; } = string.Empty;
    public string ProdutoId { get; set; } = string.Empty;
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public string IpOrigem { get; set; } = string.Empty;
    public string Fingerprint { get; set; } = string.Empty;
}