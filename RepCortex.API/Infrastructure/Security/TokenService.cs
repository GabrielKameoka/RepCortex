using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Infrastructure.Security;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GerarToken(Usuario usuario)
    {
        var chaveDiretriz = _configuration["Jwt:Secret"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        // Proteção: Se a chave estiver vazia ou com o texto padrão do Git, impede a execução falhando rápido
        if (string.IsNullOrWhiteSpace(chaveDiretriz) || chaveDiretriz.Contains("SUA_CHAVE_JWT"))
        {
            throw new InvalidOperationException(
                "🚨 CRITICAL ARCHITECTURE ERROR: A chave secreta do JWT não foi injetada pelo ambiente do desenvolvedor! " +
                "Certifique-se de configurar a variável de ambiente 'Jwt__Secret' antes de inicializar a API.");
        }

        if (string.IsNullOrWhiteSpace(issuer) || issuer.Contains("SUA_ISSUER_JWT"))
        {
            throw new InvalidOperationException(
                "🚨 CRITICAL ARCHITECTURE ERROR: O emissor do JWT não foi configurado. " +
                "Certifique-se de configurar a variável de ambiente 'Jwt__Issuer' antes de inicializar a API.");
        }

        if (string.IsNullOrWhiteSpace(audience) || audience.Contains("SUA_AUDIENCE_JWT"))
        {
            throw new InvalidOperationException(
                "🚨 CRITICAL ARCHITECTURE ERROR: A audience do JWT não foi configurada. " +
                "Certifique-se de configurar a variável de ambiente 'Jwt__Audience' antes de inicializar a API.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(chaveDiretriz);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.NomeCompleto),
                new Claim(AuthClaimTypes.TenantId, usuario.TenantId),
                new Claim(AuthClaimTypes.AccessType, AuthAccessTypes.Admin)
            }),
            Issuer = issuer,
            Audience = audience,
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}