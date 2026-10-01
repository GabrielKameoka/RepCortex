using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RepCortex.Application.Abstractions.Services;
using RepCortex.Infrastructure.Security;

namespace RepCortex.API.Middlewares;

/// <summary>
/// Propaga o tenant autenticado para o escopo da requisição.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        var tenantId = context.User.FindFirstValue(AuthClaimTypes.TenantId);
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            tenantService.DefinirTenantId(tenantId);
        }

        await _next(context);
    }
}