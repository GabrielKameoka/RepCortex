using RepCortex.Domain.Entities;

namespace RepCortex.Domain.Interfaces.Service;

/// <summary>
/// Gera token JWT para autenticação de usuário.
/// </summary>
public interface ITokenService
{
    string GerarToken(Usuario usuario);
}