using RepCortex.Domain.Entities;
using RepCortex.Application.DTOs.Dashboard;

namespace RepCortex.Application.Abstractions.Persistence;

/// <summary>
/// Contrato para persistência de avaliações. Implementação em Infrastructure.
/// </summary>
public interface IAvaliacaoRepository
{
    Task AdicionarAsync(Avaliacao avaliacao);
    Task<IEnumerable<Avaliacao>> ObterTodosAsync(string tenantId);
    Task<(IReadOnlyList<Avaliacao> Itens, int Total)> ObterPublicadasAsync(
        string tenantId, string produtoId, int pagina, int tamanhoPagina);
    Task<Avaliacao?> ObterPorIdAsync(Guid id);
    Task AtualizarAsync(Avaliacao avaliacao);
    Task<ResumoAvaliacoes> ObterResumoAsync(string tenantId);
    Task<IReadOnlyList<VolumetriaDia>> ObterVolumetriaAsync(
        string tenantId, DateTime inicioUtc, DateTime fimUtc);

    /// <summary>Verifica se um dispositivo já avaliou este produto (anti-fraude).</summary>
    Task<bool> JaAvaliouProdutoAsync(string produtoId, string fingerprint, string tenantId);
}
