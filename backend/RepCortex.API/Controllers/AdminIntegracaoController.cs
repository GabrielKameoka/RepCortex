using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RepCortex.Application.DTOs;
using RepCortex.Application.Services;
using RepCortex.API.Security;
using RepCortex.Infrastructure.Security;

namespace RepCortex.API.Controllers;

[ApiController]
[EnableCors(CorsPolicies.Dashboard)]
[Route("api/admin/integracao")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminIntegracaoController : ControllerBase
{
    private readonly TenantSettingsService _settingsService;
    private readonly AvaliacaoService _avaliacaoService;

    public AdminIntegracaoController(TenantSettingsService settingsService, AvaliacaoService avaliacaoService)
    {
        _settingsService = settingsService;
        _avaliacaoService = avaliacaoService;
    }

    [HttpGet("chave-publica")]
    public async Task<IActionResult> ObterChavePublica()
    {
        var tenantId = User.FindFirstValue(AuthClaimTypes.TenantId);
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { mensagem = "Tenant não identificado." });

        var tenant = await _settingsService.ObterAsync(tenantId);
        return tenant is null
            ? NotFound(new { mensagem = "Tenant não encontrado." })
            : Ok(new { tenant.PublishableKey });
    }

    [HttpPost("avaliacoes-teste")]
    [EnableRateLimiting("PublicWidgetPolicy")]
    public async Task<IActionResult> CriarAvaliacaoTeste([FromBody] CriarAvaliacaoRequest request)
    {
        var avaliacao = await _avaliacaoService.CriarAsync(request);

        return StatusCode(StatusCodes.Status201Created, new
        {
            avaliacao.Id,
            avaliacao.ProdutoId,
            avaliacao.Nota,
            avaliacao.Comentario,
            Status = avaliacao.Status.ToString(),
            avaliacao.Sentimento,
            avaliacao.DataCriacao
        });
    }
}
