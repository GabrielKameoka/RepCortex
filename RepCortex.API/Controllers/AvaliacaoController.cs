using Microsoft.AspNetCore.Mvc;
using RepCortex.Application.DTOs;
using RepCortex.Application.UseCases;

namespace RepCortex.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvaliacaoController : ControllerBase
{
    private readonly CriarAvaliacaoUseCase _criarAvaliacaoUseCase;

    // Recebemos o caso de uso por injeção de dependência
    public AvaliacaoController(CriarAvaliacaoUseCase criarAvaliacaoUseCase)
    {
        _criarAvaliacaoUseCase = criarAvaliacaoUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarAvaliacaoRequest request)
    {
        try
        {
            // O Controller não tem regra de negócio, ele só passa a bola pro Caso de Uso
            var avaliacaoCriada = await _criarAvaliacaoUseCase.ExecutarAsync(request);;
            
            // Retorna Status 201 (Created) e os dados da avaliação
            return StatusCode(201, avaliacaoCriada);
        }
        catch (Exception ex)
        {
            // Se o Domínio reclamar (ex: nota menor que 1), cai aqui e retorna Status 400 (Bad Request)
            return BadRequest(new { erro = ex.Message });
        }
    }
}