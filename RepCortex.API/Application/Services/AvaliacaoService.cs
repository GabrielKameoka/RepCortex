using System.Security.Claims;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AvaliacaoService(
        IAvaliacaoRepository repository,
        IAnaliseSentimentoService sentimentService,
        ITenantService tenantService,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _sentimentService = sentimentService;
        _tenantService = tenantService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Avaliacao> CriarAsync(CriarAvaliacaoRequest request)
    {
        // Tenta obter o TenantId que o TenantService configurou
        var tenantId = _tenantService.ObterTenantId();

        // Se o serviço falhar ou estiver vazio no escopo, capture direto das Claims
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id") 
                       ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        if (string.IsNullOrEmpty(tenantId))
        {
            throw new Exception("Não foi possível identificar o Tenant associado a esta requisição.");
        }

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
            request.ClienteId,
            request.UsuarioIdExterno,
            request.ProdutoId,
            request.Nota,
            request.Comentario,
            request.IpOrigem,
            request.Fingerprint,
            sentimentoEnum // <-- Agora o tipo bate 100%!
        );

        await _repository.AdicionarAsync(avaliacao);
        return avaliacao;
    }

    public async Task<IEnumerable<Avaliacao>> ObterTodasAsync()
    {
        var tenantId = _tenantService.ObterTenantId();
        return await _repository.ObterTodosAsync(tenantId);
    }
}