using RepCortex.Domain.Entities;

namespace RepCortex.Domain.Interfaces;

public interface IAvaliacaoRepository
{
    Task AdicionarAsync(Avaliacao avaliacao);
    Task<Avaliacao?> ObterPorIdAsync(Guid id);
    Task AtualizarAsync(Avaliacao avaliacao);
    
    Task<bool> JaAvaliouProdutoAsync(string produtoId, string fingerprint);
}