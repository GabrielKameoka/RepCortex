using System.ComponentModel.DataAnnotations;

namespace RepCortex.Application.DTOs;

public class CriarAvaliacaoRequest
{
    [Required]
    public string ClienteId { get; set; } = string.Empty;

    [Required]
    public string UsuarioIdExterno { get; set; } = string.Empty;

    [Required]
    public string ProdutoId { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5.")]
    public int Nota { get; set; }

    [Required]
    [MaxLength(500)]
    public string Comentario { get; set; } = string.Empty;

    public string IpOrigem { get; set; } = string.Empty;
    public string Fingerprint { get; set; } = string.Empty;
}