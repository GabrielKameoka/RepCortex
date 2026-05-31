using RepCortex.Application.DTOs;
using RepCortex.Application.Interfaces;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Entities.Enums;
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
        
        // A IA retorna uma string (ex: "Positivo")
        var sentimentoTexto = await _sentimentService.AnalisarSentimentoAsync(request.Comentario);
        
        // 2. Mapeia a string para o Enum com segurança
        // Se a IA retornar algo inesperado, o padrão "NaoAnalisado" evita quebras no sistema.
        if (!Enum.TryParse<SentimentoAvaliacao>(sentimentoTexto, true, out var sentimentoEnum))
            sentimentoEnum = SentimentoAvaliacao.NaoAnalisado;
        
        
        var novaAvaliacao = new Avaliacao(
            tenantId, 
            request.ClienteId,
            request.UsuarioIdExterno,
            request.ProdutoId,
            request.Nota,
            request.Comentario,
            request.IpOrigem,
            request.Fingerprint,
            sentimentoEnum
        );

        await _repository.AdicionarAsync(novaAvaliacao);
        return novaAvaliacao;
    }
}