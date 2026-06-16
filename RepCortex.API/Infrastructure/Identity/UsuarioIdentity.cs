using Microsoft.AspNetCore.Identity;

namespace RepCortex.Infrastructure.Identity;

public class UsuarioIdentity : IdentityUser
{
    public string NomeCompleto { get; set; }
    public string TenantId { get; set; }
    public DateTime DataCadastro { get; set; }
}