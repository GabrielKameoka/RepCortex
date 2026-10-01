using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RepCortex.Application.Services;
using RepCortex.API.Security;
using RepCortex.Infrastructure.Security;

namespace RepCortex.API.Controllers;

[ApiController]
[EnableCors(CorsPolicies.Dashboard)]
[Route("api/admin/dashboard")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminDashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public AdminDashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("metricas")]
    public async Task<IActionResult> ObterMetricasIniciais()
    {
        var tenantId = User.FindFirstValue(AuthClaimTypes.TenantId);

        if (string.IsNullOrEmpty(tenantId))
            return BadRequest(new { mensagem = "Inquilino não identificado." });

        return Ok(await _dashboardService.ObterMetricasAsync(tenantId));
    }
}
