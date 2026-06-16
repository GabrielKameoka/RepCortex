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
        
        // Proteção: Se a chave estiver vazia ou com o texto padrão do Git, impede a execução falhando rápido
        if (string.IsNullOrWhiteSpace(chaveDiretriz) || chaveDiretriz.Contains("SUA_CHAVE_JWT"))
        {
            throw new InvalidOperationException(
                "🚨 CRITICAL ARCHITECTURE ERROR: A chave secreta do JWT não foi injetada pelo ambiente do desenvolvedor! " +
                "Certifique-se de configurar a variável de ambiente 'Jwt__Secret' antes de inicializar a API.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(chaveDiretriz);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.NomeCompleto),
                new Claim("TenantId", usuario.TenantId) // Injetando o isolamento do tenant dentro do token assinado
            }),
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