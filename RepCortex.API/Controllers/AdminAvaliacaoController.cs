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
            a.DataCriacao,
            a.Resposta
        });

        return Ok(resposta);
    }

    [HttpPost("{id}/aprovar")]
    public async Task<IActionResult> Aprovar(Guid id)
    {
        try
        {
            await _avaliacaoService.AprovarAsync(id);
            return Ok(new { sucesso = true, mensagem = "Avaliação aprovada com sucesso!" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { sucesso = false, mensagem = "Avaliação não encontrada." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id}/rejeitar")]
    public async Task<IActionResult> Rejeitar(Guid id)
    {
        try
        {
            await _avaliacaoService.RejeitarAsync(id);
            return Ok(new { sucesso = true, mensagem = "Avaliação rejeitada com sucesso!" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { sucesso = false, mensagem = "Avaliação não encontrada." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    public class ResponderAvaliacaoRequest
    {
        public string Resposta { get; set; } = string.Empty;
    }

    [HttpPost("{id}/responder")]
    public async Task<IActionResult> Responder(Guid id, [FromBody] ResponderAvaliacaoRequest request)
    {
        try
        {
            await _avaliacaoService.ResponderAsync(id, request.Resposta);
            return Ok(new { sucesso = true, mensagem = "Comentário respondido com sucesso!" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { sucesso = false, mensagem = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { sucesso = false, mensagem = "Avaliação não encontrada." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
