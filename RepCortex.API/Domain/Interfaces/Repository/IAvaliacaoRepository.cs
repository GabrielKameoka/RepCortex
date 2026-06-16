using RepCortex.Domain.Entities;

namespace RepCortex.Domain.Interfaces.Repository;

/// <summary>
/// Contrato para persistência de avaliações. Implementação em Infrastructure.
/// </summary>
public interface IAvaliacaoRepository
{
    Task AdicionarAsync(Avaliacao avaliacao);
    Task<IEnumerable<Avaliacao>> ObterTodosAsync(string tenantId);
    Task<Avaliacao?> ObterPorIdAsync(Guid id);
    Task AtualizarAsync(Avaliacao avaliacao);
    
    /// <summary>Verifica se um dispositivo já avaliou este produto (anti-fraude).</summary>
    Task<bool> JaAvaliouProdutoAsync(string produtoId, string fingerprint, string tenantId);
}