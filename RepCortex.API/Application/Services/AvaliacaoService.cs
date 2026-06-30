using RepCortex.Application.DTOs;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Entities.Enums;
using RepCortex.Domain.Interfaces.Repository;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Application.Services;

/// <summary>
/// Serviço consolidado para gestão de avaliações.
/// </summary>
public class AvaliacaoService
{
    private readonly IAvaliacaoRepository _repository;
    private readonly IAnaliseSentimentoService _sentimentService;
    private readonly ITenantService _tenantService;

    public AvaliacaoService(
        IAvaliacaoRepository repository,
        IAnaliseSentimentoService sentimentService,
        ITenantService tenantService)
    {
        _repository = repository;
        _sentimentService = sentimentService;
        _tenantService = tenantService;
    }

    public async Task<Avaliacao> CriarAsync(CriarAvaliacaoRequest request)
    {
        // Tenta obter pelo Middleware/Header. Se vier nulo ou vazio, usa o do JSON/Request.
        var tenantId = _tenantService.ObterTenantId();
    
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            tenantId = request.TenantId;
        }

        // Se mesmo assim for nulo, lança uma exceção amigável antes de quebrar no domínio
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Não foi possível identificar o Tenant através do cabeçalho de API ou do corpo da requisição.");
        }

        var jaAvaliou = await _repository.JaAvaliouProdutoAsync(
            request.ProdutoId,
            request.Fingerprint,
            tenantId
        );

        if (jaAvaliou)
        {
            throw new InvalidOperationException("Este dispositivo já enviou uma avaliação para este produto.");
        }

        var sentimentoTexto = await _sentimentService.AnalisarSentimentoAsync(request.Comentario);

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

    public async Task<IEnumerable<Avaliacao>> ObterTodasAsync()
    {
        var tenantId = _tenantService.ObterTenantId();
        return await _repository.ObterTodosAsync(tenantId);
    }

    public async Task AprovarAsync(Guid id)
    {
        var avaliacao = await _repository.ObterPorIdAsync(id);
        if (avaliacao == null)
            throw new KeyNotFoundException("Avaliação não encontrada.");

        var tenantId = _tenantService.ObterTenantId();
        if (avaliacao.TenantId != tenantId)
            throw new UnauthorizedAccessException("Acesso negado.");

        avaliacao.Aprovar();
        await _repository.AtualizarAsync(avaliacao);
    }

    public async Task RejeitarAsync(Guid id)
    {
        var avaliacao = await _repository.ObterPorIdAsync(id);
        if (avaliacao == null)
            throw new KeyNotFoundException("Avaliação não encontrada.");

        var tenantId = _tenantService.ObterTenantId();
        if (avaliacao.TenantId != tenantId)
            throw new UnauthorizedAccessException("Acesso negado.");

        avaliacao.Rejeitar();
        await _repository.AtualizarAsync(avaliacao);
    }

    public async Task ResponderAsync(Guid id, string resposta)
    {
        var avaliacao = await _repository.ObterPorIdAsync(id);
        if (avaliacao == null)
            throw new KeyNotFoundException("Avaliação não encontrada.");

        var tenantId = _tenantService.ObterTenantId();
        if (avaliacao.TenantId != tenantId)
            throw new UnauthorizedAccessException("Acesso negado.");

        avaliacao.Responder(resposta);
        await _repository.AtualizarAsync(avaliacao);
    }
}