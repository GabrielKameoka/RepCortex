using System.ComponentModel.DataAnnotations;

namespace RepCortex.Application.DTOs;

public class CriarAvaliacaoRequest
{
    [Required]
    [MaxLength(100)]
    public string ClienteId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string UsuarioIdExterno { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ProdutoId { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5.")]
    public int Nota { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Comentario { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Fingerprint { get; set; } = string.Empty;
}
