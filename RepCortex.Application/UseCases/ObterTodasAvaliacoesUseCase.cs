using RepCortex.Application.Interfaces;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Application.UseCases;

public class ObterTodasAvaliacoesUseCase : IObterTodasAvaliacoesUseCase
{
    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly ITenantService _tenantService;

    public ObterTodasAvaliacoesUseCase(IAvaliacaoRepository avaliacaoRepository, ITenantService tenantService)
    {
        _avaliacaoRepository = avaliacaoRepository;
        _tenantService = tenantService;
    }

    public async Task<IEnumerable<Avaliacao>> ExecutarAsync()
    {
        var tenantId = _tenantService.ObterTenantId();
        return await _avaliacaoRepository.ObterTodosAsync(tenantId);
    }
}