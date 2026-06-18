using System.Threading.Tasks;
using RepCortex.Domain.Entities;

namespace RepCortex.Domain.Interfaces.Service;

/// <summary>
/// Serviço para registro e login de usuários.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Registra novo usuário. Retorna (Sucesso, Erro, UsuarioId).
    /// Se sucesso=true, usuarioId contém o ID gerado. Senão, erro descreve o problema.
    /// </summary>
    Task<(bool Sucesso, string? Erro, string? UsuarioId)> RegistrarUsuarioAsync(Usuario usuario, string senha);
    
    /// <summary>
    /// Faz login com email e senha. Retorna (Sucesso, Token, Erro).
    /// Se sucesso=true, Token contém JWT válido. Senão, erro descreve o problema.
    /// </summary>
    Task<(bool Sucesso, string? Token, string? Erro)> LoginAsync(string tenantId, string email, string senha);
}