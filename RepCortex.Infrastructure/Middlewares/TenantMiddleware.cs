using Microsoft.AspNetCore.Http;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Infrastructure.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;
    
    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // SEMPRE sabe que o dado vai na header da requisição
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
        {
            tenantService.DefinirTenantId(tenantId!);
        }
        
        await _next(context);
    }
}