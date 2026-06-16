using Microsoft.AspNetCore.Http;
using RepCortex.Domain.Interfaces.Service;
using RepCortex.Domain.Interfaces.Repository;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace RepCortex.Infrastructure.Middlewares;

/// <summary>
/// Intercepta a requisição para definir o contexto do Tenant de forma segura (JWT ou API Key pública).
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;
    
    public async Task InvokeAsync(HttpContext context, ITenantService tenantService, ITenantRepository tenantRepository)
    {
        var path = context.Request.Path;

        // Libera rotas públicas de Auth e a documentação do Scalar/OpenAPI
        if (path.StartsWithSegments("/api/auth") || 
            path.StartsWithSegments("/scalar") || 
            path.StartsWithSegments("/openapi"))
        {
            await _next(context);
            return;
        }

        string? tenantIdIdenficado = null; 

        // 1. Tenta extrair o TenantId de forma segura do Token JWT (Autenticação do Admin)
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var tokenString = authHeader.Substring("Bearer ".Length).Trim();
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(tokenString))
                {
                    var jwtToken = handler.ReadJwtToken(tokenString);
                    tenantIdIdenficado = jwtToken.Claims
                        .FirstOrDefault(c => c.Type == "TenantId" || c.Type == "tenantId")?.Value;
                }
            }
            catch
            {
                // Se o token estiver malformado, deixa o pipeline seguir
            }
        }

        // 2. Se não veio em JWT, tenta autenticar via X-Api-Key (Integração Pública do Cliente Final)
        if (string.IsNullOrWhiteSpace(tenantIdIdenficado))
        {
            var apiKey = context.Request.Headers["X-Api-Key"].ToString();
            if (!string.IsNullOrEmpty(apiKey))
            {
                var tenant = await tenantRepository.ObterPorApiKeyAsync(apiKey);
                if (tenant != null && tenant.Ativo)
                {
                    // Validação de Domínio (CORS / Origin)
                    var origin = context.Request.Headers["Origin"].ToString();
                    if (string.IsNullOrEmpty(origin))
                    {
                        origin = context.Request.Headers["Referer"].ToString();
                    }

                    if (!string.IsNullOrEmpty(origin))
                    {
                        try
                        {
                            var uri = new Uri(origin);
                            var host = uri.Host.ToLower();

                            var dominios = tenant.DominiosAutorizados.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                .Select(d => d.Trim().ToLower());

                            bool autorizado = dominios.Contains("*") || 
                                             dominios.Contains(host) || 
                                             host == "localhost" || 
                                             host == "127.0.0.1";

                            if (!autorizado)
                            {
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                context.Response.ContentType = "application/json";
                                var respostaErro = new { mensagem = $"Acesso negado. O domínio '{host}' não está autorizado para esta API Key." };
                                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(respostaErro));
                                return;
                            }
                        }
                        catch
                        {
                            // Se a URI for malformada, segue fluxo
                        }
                    }

                    tenantIdIdenficado = tenant.Id;
                }
            }
        }

        // Se encontrou o Tenant por alguma das vias, injeta no escopo e avança
        if (!string.IsNullOrWhiteSpace(tenantIdIdenficado))
        {
            tenantService.DefinirTenantId(tenantIdIdenficado);
            await _next(context);
            return;
        }

        // Se chegou aqui, não há nenhuma identificação válida de empresa
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        
        var respostaSemIdentificacao = new { mensagem = "Acesso negado. Identificação da empresa (Tenant) não fornecida. Envie um Token JWT ou o cabeçalho 'X-Api-Key' com uma chave pública válida." };
        var jsonString = System.Text.Json.JsonSerializer.Serialize(respostaSemIdentificacao);
        await context.Response.WriteAsync(jsonString);
    }
}