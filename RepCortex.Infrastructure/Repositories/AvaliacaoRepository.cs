using Microsoft.EntityFrameworkCore;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces;
using RepCortex.Infrastructure.Data;

namespace RepCortex.Infrastructure.Repositories;

public class AvaliacaoRepository : IAvaliacaoRepository
{
    private readonly AppDbContext _context;

    public AvaliacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Avaliacao avaliacao)
    {
        await _context.Avaliacoes.AddAsync(avaliacao);
        await _context.SaveChangesAsync();
    }
    
    public async Task<Avaliacao?> ObterPorIdAsync(Guid id)
    {
        return await _context.Avaliacoes.FindAsync(id);
    }

    public async Task AtualizarAsync(Avaliacao avaliacao)
    {
        _context.Avaliacoes.Update(avaliacao);
        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> JaAvaliouProdutoAsync(string produtoId, string fingerprint)
    {
        // checa se existe alguma linha com esse Produto E esse Fingerprint
        return await _context.Avaliacoes
            .AnyAsync(a => a.ProdutoId == produtoId && a.Fingerprint == fingerprint);
    }
}