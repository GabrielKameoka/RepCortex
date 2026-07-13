using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RepCortex.Application.DTOs;
using RepCortex.Application.Services;
using RepCortex.Infrastructure.Security;

namespace RepCortex.API.Controllers;

[ApiController]
[Route("api/public/avaliacoes")]
[Authorize(Policy = AuthPolicies.PublicIngestOnly)] // Reativado: Exige X-Api-Key válida
[EnableRateLimiting("PublicWidgetPolicy")]          // Reativado: Ativa a proteção do Redis contra spam
public class PublicAvaliacaoController : ControllerBase
{
    private readonly AvaliacaoService _avaliacaoService;

    public PublicAvaliacaoController(AvaliacaoService avaliacaoService)
    {
        _avaliacaoService = avaliacaoService;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarAvaliacaoRequest request)
    {
        var avaliacao = await _avaliacaoService.CriarAsync(request);

        return StatusCode(201, new
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