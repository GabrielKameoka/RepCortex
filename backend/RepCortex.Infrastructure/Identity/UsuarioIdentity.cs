using Microsoft.AspNetCore.Identity;

namespace RepCortex.Infrastructure.Identity;

public class UsuarioIdentity : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
}
