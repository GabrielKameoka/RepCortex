using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RepCortex.API.Application.DTOs.Dashboard;
using RepCortex.Infrastructure.Security;
using RepCortex.Domain.Interfaces.Repository;
using RepCortex.Domain.Entities.Enums;

namespace RepCortex.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminDashboardController : ControllerBase
{
    private readonly IAvaliacaoRepository _repository;

    public AdminDashboardController(IAvaliacaoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("metricas")]
    public async Task<IActionResult> ObterMetricasIniciais()
    {
        var tenantId = User.FindFirstValue(AuthClaimTypes.TenantId);
        
        if (string.IsNullOrEmpty(tenantId))
            return BadRequest(new { mensagem = "Inquilino não identificado." });

        var todas = await _repository.ObterTodosAsync(tenantId);
        var lista = todas.ToList();

        var metricas = new TenantDashboardMetrics(
            TotalAvaliacoes: lista.Count,
            MediaNotas: lista.Any() ? Math.Round(lista.Average(a => a.Nota), 1) : 0,
            TotalPositivas: lista.Count(a => a.Sentimento == SentimentoAvaliacao.Positivo),
            TotalNeutras: lista.Count(a => a.Sentimento == SentimentoAvaliacao.Neutro),
            TotalNegativas: lista.Count(a => a.Sentimento == SentimentoAvaliacao.Negativo),
            TotalPendentesModeracao: lista.Count(a => a.Status.ToString() == "Pendente"),
            VolumetriaUltimosDias: lista
                .GroupBy(a => a.DataCriacao.ToString("dd/MM"))
                .OrderBy(g => g.Key)
                .Take(7)
                .Select(g => new GraficoLinhaPonto(g.Key, g.Count()))
                .ToList()
        );

        return Ok(metricas);
    }
}