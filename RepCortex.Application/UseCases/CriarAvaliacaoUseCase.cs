using RepCortex.Application.DTOs;
using RepCortex.Application.Interfaces;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Application.UseCases;

public class CriarAvaliacaoUseCase : ICriarAvaliacaoUseCase
{
    private readonly IAvaliacaoRepository _repository;
    private readonly IAnaliseSentimentoService _sentimentService;
    private readonly ITenantService _tenantService; 

    
    public CriarAvaliacaoUseCase(
        IAvaliacaoRepository repository,
        IAnaliseSentimentoService sentimentService,
        ITenantService tenantService)
    {
        _repository = repository;
        _sentimentService = sentimentService;
        _tenantService = tenantService; 
    }

    public async Task<Avaliacao> ExecutarAsync(CriarAvaliacaoRequest request)
    {
        var tenantId = _tenantService.ObterTenantId();
        
        var jaAvaliou = await _repository.JaAvaliouProdutoAsync(
            request.ProdutoId,
            request.Fingerprint,
            tenantId 
        );

        if (jaAvaliou)
        {
            throw new InvalidOperationException(
                "Erro de validação: Este dispositivo já enviou uma avaliação para este produto.");
        }
        
        var sentimentoDetectado = await _sentimentService.AnalisarSentimentoAsync(request.Comentario);
        
        var novaAvaliacao = new Avaliacao(
            tenantId, 
            request.ClienteId,
            request.UsuarioIdExterno,
            request.ProdutoId,
            request.Nota,
            request.Comentario,
            request.IpOrigem,
            request.Fingerprint,
            sentimentoDetectado
        );

        await _repository.AdicionarAsync(novaAvaliacao);
        return novaAvaliacao;
    }
}