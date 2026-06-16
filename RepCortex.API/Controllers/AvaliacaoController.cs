using Microsoft.AspNetCore.Mvc;
using RepCortex.Application.DTOs;
using RepCortex.Application.Services;

namespace RepCortex.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvaliacaoController : ControllerBase
{
    private readonly AvaliacaoService _avaliacaoService;

    public AvaliacaoController(AvaliacaoService avaliacaoService)
    {
        _avaliacaoService = avaliacaoService;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarAvaliacaoRequest request)
    {
        var avaliacao = await _avaliacaoService.CriarAsync(request);
        
        return StatusCode(201, new {
            avaliacao.Id,
            avaliacao.ProdutoId,
            avaliacao.Nota,
            avaliacao.Comentario,
            Status = avaliacao.Status.ToString(),
            avaliacao.Sentimento,
            avaliacao.DataCriacao
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var avaliacoes = await _avaliacaoService.ObterTodasAsync();
        
        var resposta = avaliacoes.Select(a => new {
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