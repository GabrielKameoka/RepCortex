using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces.Service; // Certifique-se de que este namespace bate com suas pastas
using RepCortex.Infrastructure.Identity;

namespace RepCortex.Infrastructure.Security;

public class IdentityService : IIdentityService
{
    private readonly UserManager<UsuarioIdentity> _userManagerNative;
    private readonly ITokenService _tokenService;

    public IdentityService(UserManager<UsuarioIdentity> userManagerNative, ITokenService tokenService)
    {
        _userManagerNative = userManagerNative;
        _tokenService = tokenService;
    }

    public async Task<(bool Sucesso, string? Erro, string? UsuarioId)> RegistrarUsuarioAsync(Usuario usuario, string senha)
    {
        var identityUser = new UsuarioIdentity
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            UserName = usuario.Email, 
            TenantId = usuario.TenantId,
            DataCadastro = usuario.DataCadastro
        };

        var resultado = await _userManagerNative.CreateAsync(identityUser, senha);

        if (resultado.Succeeded)
        {
            return (true, null, identityUser.Id);
        }

        var primeiroErro = resultado.Errors.FirstOrDefault()?.Description ?? "Erro desconhecido ao criar usuário.";
        return (false, primeiroErro, null);
    }

    public async Task<(bool Sucesso, string? Token, string? Erro)> LoginAsync(string email, string senha)
    {
        var usuarioIdentity = await _userManagerNative.FindByEmailAsync(email);
    
        if (usuarioIdentity == null)
            return (false, null, "Credenciais inválidas.");

        var senhaValida = await _userManagerNative.CheckPasswordAsync(usuarioIdentity, senha);
        if (!senhaValida)
            return (false, null, "Credenciais inválidas.");

        // CONVERSÃO: Transformamos o modelo do banco no modelo rico do Domínio
        var usuarioDominio = new Usuario(
            usuarioIdentity.Id, 
            usuarioIdentity.NomeCompleto, 
            usuarioIdentity.Email!, 
            usuarioIdentity.TenantId
        );

        // Agora passamos o objeto de Domínio puro, respeitando o contrato!
        var token = _tokenService.GerarToken(usuarioDominio);
        return (true, token, null);
    }
}