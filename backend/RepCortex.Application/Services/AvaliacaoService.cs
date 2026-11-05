using RepCortex.Application.Abstractions.Context;
using RepCortex.Application.DTOs;
using RepCortex.Application.DTOs.Public;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Entities.Enums;
using RepCortex.Application.Abstractions.Persistence;
using RepCortex.Application.Abstractions.Services;

namespace RepCortex.Application.Services;

/// <summary>
/// Serviço consolidado para gestão de avaliações.
/// </summary>
public class AvaliacaoService
{
    private readonly IAvaliacaoRepository _repository;
    private readonly IAnaliseSentimentoService _sentimentService;
    private readonly ITenantService _tenantService;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;
    private readonly IDashboardEventPublisher _dashboardEvents;

    public AvaliacaoService(
        IAvaliacaoRepository repository,
        IAnaliseSentimentoService sentimentService,
        ITenantService tenantService,
        ITenantRepository tenantRepository,
        IRequestContext requestContext,
        IDashboardEventPublisher dashboardEvents)
    {
        _repository = repository;
        _sentimentService = sentimentService;
        _tenantService = tenantService;
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
        _dashboardEvents = dashboardEvents;
    }

    public async Task<Avaliacao> CriarAsync(CriarAvaliacaoRequest request)
    {
        var tenantId = _tenantService.ObterTenantId();
        var tenant = await _tenantRepository.ObterPorIdAsync(tenantId)
            ?? throw new KeyNotFoundException("Tenant não encontrado.");

        var ipOrigem = _requestContext.RemoteIpAddress;

        // 1. Executa a análise de sentimento da IA (Retorna string)
        string sentimentoString = await _sentimentService.AnalisarSentimentoAsync(request.Comentario);

        // 2. Converte a string da IA para o tipo exato do seu Enum (Ignorando letras maiúsculas/minúsculas)
        if (!Enum.TryParse<SentimentoAvaliacao>(sentimentoString, true, out var sentimentoEnum))
        {
            // Caso a IA devolva algo inesperado, define um valor padrão seguro
            sentimentoEnum = SentimentoAvaliacao.Neutro; 
        }

        // 3. Instancia a entidade passando o Enum convertido perfeitamente
        var avaliacao = new Avaliacao(
            tenantId, 
            request.UsuarioIdExterno,
            request.ProdutoId,
            request.Nota,
            request.Comentario,
            ipOrigem,
            request.Fingerprint,
            sentimentoEnum,
            tenant.PoliticaModeracao,
            request.NomeUsuarioExterno
        );

        await _repository.AdicionarAsync(avaliacao);
        await _dashboardEvents.PublicarAtualizacaoAsync(tenantId);
        return avaliacao;
    }

    public async Task<IEnumerable<Avaliacao>> ObterTodasAsync()
    {
        var tenantId = _tenantService.ObterTenantId();
        return await _repository.ObterTodosAsync(tenantId);
    }

    public async Task<ListaAvaliacoesPublicasResponse> ObterPublicadasAsync(
        string produtoId, int pagina, int tamanhoPagina)
    {
        if (pagina < 1)
            throw new ArgumentException("A página deve ser maior que zero.");

        if (tamanhoPagina is < 1 or > 50)
            throw new ArgumentException("O tamanho da página deve estar entre 1 e 50.");

        var tenantId = _tenantService.ObterTenantId();
        var (itens, total) = await _repository.ObterPublicadasAsync(
            tenantId, produtoId, pagina, tamanhoPagina);

        var resposta = itens
            .Select(a => new AvaliacaoPublicaResponse(
                a.Id, a.ProdutoId, a.Nota, a.Comentario, a.Resposta, a.DataCriacao))
            .ToList();

        return new ListaAvaliacoesPublicasResponse(
            resposta,
            pagina,
            tamanhoPagina,
            total,
            (int)Math.Ceiling(total / (double)tamanhoPagina));
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
        await _dashboardEvents.PublicarAtualizacaoAsync(tenantId);
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
        await _dashboardEvents.PublicarAtualizacaoAsync(tenantId);
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
        await _dashboardEvents.PublicarAtualizacaoAsync(tenantId);
    }
}
