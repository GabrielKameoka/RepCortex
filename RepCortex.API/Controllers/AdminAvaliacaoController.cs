using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepCortex.Application.Services;
using RepCortex.Infrastructure.Security;

namespace RepCortex.API.Controllers;

[ApiController]
[Route("api/admin/avaliacoes")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminAvaliacaoController : ControllerBase
{
    private readonly AvaliacaoService _avaliacaoService;

    public AdminAvaliacaoController(AvaliacaoService avaliacaoService)
    {
        _avaliacaoService = avaliacaoService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var avaliacoes = await _avaliacaoService.ObterTodasAsync();

        var resposta = avaliacoes.Select(a => new
        {
            a.Id,
            a.ProdutoId,
            a.Nota,
            a.Comentario,
            Status = a.Status.ToString(),
            a.Sentimento,
            a.DataCriacao
        });

        return Ok(resposta);
    }
}
