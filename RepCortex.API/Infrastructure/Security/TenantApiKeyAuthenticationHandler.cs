using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces.Repository;

namespace RepCortex.Infrastructure.Security;

public class TenantApiKeyAuthenticationHandler : AuthenticationHandler<TenantApiKeyAuthenticationOptions>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IHostEnvironment _hostEnvironment;

    public TenantApiKeyAuthenticationHandler(
        IOptionsMonitor<TenantApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ITenantRepository tenantRepository,
        IHostEnvironment hostEnvironment) : base(options, logger, encoder)
    {
        _tenantRepository = tenantRepository;
        _hostEnvironment = hostEnvironment;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
        {
            return AuthenticateResult.NoResult();
        }

        var apiKey = apiKeyHeader.ToString().Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var tenant = Options.KeyType switch
        {
            TenantApiKeyType.Publishable => await _tenantRepository.ObterPorPublishableKeyAsync(apiKey),
            TenantApiKeyType.Secret => await _tenantRepository.ObterPorSecretKeyAsync(apiKey),
            _ => null
        };

        if (tenant is null || !tenant.Ativo)
        {
            return AuthenticateResult.Fail("Chave de API inválida ou desativada.");
        }

        if (Options.KeyType == TenantApiKeyType.Publishable && !OrigemPermitida(tenant))
        {
            return AuthenticateResult.Fail("A origem desta chave pública não está autorizada.");
        }

        var accessType = Options.KeyType == TenantApiKeyType.Publishable
            ? AuthAccessTypes.Publishable
            : AuthAccessTypes.Secret;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, tenant.Id),
            new Claim(AuthClaimTypes.TenantId, tenant.Id),
            new Claim(AuthClaimTypes.AccessType, accessType)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private bool OrigemPermitida(Tenant tenant)
    {
        var origem = Request.Headers.Origin.ToString();
        if (string.IsNullOrWhiteSpace(origem))
        {
            origem = Request.Headers.Referer.ToString();
        }

        if (string.IsNullOrWhiteSpace(origem))
        {
            return _hostEnvironment.IsDevelopment();
        }

        if (!Uri.TryCreate(origem, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var host = uri.Host.ToLowerInvariant();
        var dominiosAutorizados = tenant.DominiosAutorizados
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(dominio => dominio.Trim().ToLowerInvariant());

        return dominiosAutorizados.Contains("*") ||
               dominiosAutorizados.Contains(host) ||
               (_hostEnvironment.IsDevelopment() && (host == "localhost" || host == "127.0.0.1"));
    }
}
