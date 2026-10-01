using RepCortex.Domain.Entities;

namespace RepCortex.Application.Abstractions.Services;

/// <summary>
/// Gera token JWT para autenticação de usuário.
/// </summary>
public interface ITokenService
{
    string GerarToken(Usuario usuario);
}