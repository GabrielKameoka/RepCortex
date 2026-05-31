using Microsoft.AspNetCore.Mvc;
using RepCortex.Application.DTOs;
using RepCortex.Application.Interfaces;

namespace RepCortex.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvaliacaoController : ControllerBase
{
    private readonly ICriarAvaliacaoUseCase _criarAvaliacaoUseCase; 
    private readonly IObterTodasAvaliacoesUseCase  _obterTodasAvaliacoesUseCase;

    public AvaliacaoController(ICriarAvaliacaoUseCase criarAvaliacaoUseCase, IObterTodasAvaliacoesUseCase obterTodasAvaliacoesUseCase) // <- Injeta a Interface
    {
        _criarAvaliacaoUseCase = criarAvaliacaoUseCase;
        _obterTodasAvaliacoesUseCase = obterTodasAvaliacoesUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Executar([FromBody] CriarAvaliacaoRequest request)
    {
        try
        {
            var avaliacao = await _criarAvaliacaoUseCase.ExecutarAsync(request);
        
            // Em vez de passar a entidade 'avaliacao' inteira, 
            // criamos um objeto de saída limpo e seguro:
            var resposta = new {
                Id = avaliacao.Id,
                ProdutoId = avaliacao.ProdutoId,
                Nota = avaliacao.Nota,
                Comentario = avaliacao.Comentario,
                Status = avaliacao.Status.ToString(), // Transforma o Enum em texto ("Pendente")
                Sentimento = avaliacao.Sentimento,
                DataCriacao = avaliacao.DataCriacao
            };
        
            return StatusCode(201, resposta);
        }
        catch (Exception ex)
        {
            return BadRequest(new { erro = ex.Message, linha = ex.StackTrace });
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        try
        {
            var avaliacoes = await _obterTodasAvaliacoesUseCase.ExecutarAsync();
        
            // Mapeia para uma resposta segura (ocultando IP e Fingerprint)
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
        catch (Exception ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }
}