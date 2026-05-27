using RepCortex.Application.DTOs;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Application.UseCases;

public class CriarAvaliacaoUseCase
{
    private readonly IAvaliacaoRepository _repository;
    private readonly IAnaliseSentimentoService _sentimentService; // <- Injetando a IA

    public CriarAvaliacaoUseCase(IAvaliacaoRepository repository, IAnaliseSentimentoService sentimentService)
    {
        _repository = repository;
        _sentimentService = sentimentService;
    }

    public async Task<Avaliacao> ExecutarAsync(CriarAvaliacaoRequest request)
    {
        // 1. Barreira Anti-Fraude
        var jaAvaliou = await _repository.JaAvaliouProdutoAsync(request.ProdutoId, request.Fingerprint);
        if (jaAvaliou)
        {
            throw new InvalidOperationException("Erro de validação: Este dispositivo já enviou uma avaliação para este produto.");
        }

        // 2. Chamada para o Serviço de Inteligência Artificial local
        var sentimentoDetectado = await _sentimentService.AnalisarSentimentoAsync(request.Comentario);

        // 3. Criação da Entidade passando o resultado da IA
        var novaAvaliacao = new Avaliacao(
            request.ClienteId,
            request.UsuarioIdExterno,
            request.ProdutoId,
            request.Nota,
            request.Comentario,
            request.IpOrigem,
            request.Fingerprint,
            sentimentoDetectado // <- Passando para o construtor rico
        );

        // 4. Salva no banco de dados
        await _repository.AdicionarAsync(novaAvaliacao);
        return novaAvaliacao;
    }
}