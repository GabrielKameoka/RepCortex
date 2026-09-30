using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RepCortex.Application.DTOs.Tenant;
using RepCortex.Domain.Entities.Enums;
using RepCortex.Domain.Interfaces.Repository;
using RepCortex.Infrastructure.Security;

namespace RepCortex.API.Controllers;

[ApiController]
[EnableCors(CorsPolicies.Dashboard)]
[Route("api/admin/configuracoes/moderacao")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminModeracaoController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;

    public AdminModeracaoController(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var tenantId = User.FindFirstValue(AuthClaimTypes.TenantId);
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { mensagem = "Tenant não identificado." });

        var tenant = await _tenantRepository.ObterPorIdAsync(tenantId);
        return tenant is null
            ? NotFound(new { mensagem = "Tenant não encontrado." })
            : Ok(new PoliticaModeracaoResponse(tenant.PoliticaModeracao));
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarPoliticaModeracaoRequest request)
    {
        if (!Enum.IsDefined(request.Politica))
            return BadRequest(new { mensagem = "Política de moderação inválida." });

        var tenantId = User.FindFirstValue(AuthClaimTypes.TenantId);
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { mensagem = "Tenant não identificado." });

        var tenant = await _tenantRepository.ObterPorIdAsync(tenantId);
        if (tenant is null)
            return NotFound(new { mensagem = "Tenant não encontrado." });

        tenant.DefinirPoliticaModeracao(request.Politica);
        await _tenantRepository.AtualizarAsync(tenant);

        return Ok(new PoliticaModeracaoResponse(tenant.PoliticaModeracao));
    }
}
