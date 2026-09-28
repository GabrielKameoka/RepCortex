using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RepCortex.Infrastructure.Security;

namespace RepCortex.API.Hubs;

[Authorize(Policy = AuthPolicies.AdminOnly)]
public class DashboardHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirstValue(AuthClaimTypes.TenantId);
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            // Adiciona a conexão do lojista ao grupo específico do seu tenant
            await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = Context.User?.FindFirstValue(AuthClaimTypes.TenantId);
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}