using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RepCortex.Domain.Interfaces.Repository;
using RepCortex.Infrastructure.Security;

namespace RepCortex.API.Controllers;

[ApiController]
[EnableCors(CorsPolicies.Dashboard)]
[Route("api/admin/integracao")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminIntegracaoController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;

    public AdminIntegracaoController(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    [HttpGet("chave-publica")]
    public async Task<IActionResult> ObterChavePublica()
    {
        var tenantId = User.FindFirstValue(AuthClaimTypes.TenantId);
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { mensagem = "Tenant não identificado." });

        var tenant = await _tenantRepository.ObterPorIdAsync(tenantId);
        return tenant is null
            ? NotFound(new { mensagem = "Tenant não encontrado." })
            : Ok(new { tenant.PublishableKey });
    }
}
