using Microsoft.EntityFrameworkCore;
using RepCortex.Domain.Entities;
using RepCortex.Application.Abstractions.Persistence;
using RepCortex.Infrastructure.Data;
using RepCortex.Application.DTOs.Dashboard;
using RepCortex.Domain.Entities.Enums;

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


    public async Task<IEnumerable<Avaliacao>> ObterTodosAsync(string tenantId)
    {
        return await _context.Avaliacoes
            .Where(a => a.TenantId == tenantId) // isolamento por inquilino(tenant)
            .AsNoTracking() // melhor a performance
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Avaliacao> Itens, int Total)> ObterPublicadasAsync(
        string tenantId, string produtoId, int pagina, int tamanhoPagina)
    {
        var query = _context.Avaliacoes
            .Where(a => a.TenantId == tenantId &&
                        a.ProdutoId == produtoId &&
                        a.Status == Domain.Entities.Enums.StatusAvaliacao.Aprovada)
            .AsNoTracking()
            .OrderByDescending(a => a.DataCriacao);

        var total = await query.CountAsync();
        var itens = await query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
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

    public async Task<ResumoAvaliacoes> ObterResumoAsync(string tenantId)
    {
        var resumo = await _context.Avaliacoes.AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .GroupBy(_ => 1)
            .Select(grupo => new ResumoAvaliacoes(
                grupo.Count(), grupo.Average(a => (double)a.Nota),
                grupo.Count(a => a.Sentimento == SentimentoAvaliacao.Positivo),
                grupo.Count(a => a.Sentimento == SentimentoAvaliacao.Neutro),
                grupo.Count(a => a.Sentimento == SentimentoAvaliacao.Negativo),
                grupo.Count(a => a.Status == StatusAvaliacao.Pendente)))
            .FirstOrDefaultAsync();

        return resumo ?? new ResumoAvaliacoes(0, 0, 0, 0, 0, 0);
    }

    public async Task<IReadOnlyList<VolumetriaDia>> ObterVolumetriaAsync(
        string tenantId, DateTime inicioUtc, DateTime fimUtc)
    {
        var dias = await _context.Avaliacoes.AsNoTracking()
            .Where(a => a.TenantId == tenantId &&
                        a.DataCriacao >= inicioUtc && a.DataCriacao < fimUtc)
            .GroupBy(a => a.DataCriacao.Date)
            .Select(grupo => new { DiaUtc = grupo.Key, Quantidade = grupo.Count() })
            .ToListAsync();

        return dias.OrderBy(item => item.DiaUtc)
            .Select(item => new VolumetriaDia(item.DiaUtc, item.Quantidade))
            .ToList();
    }

    public async Task<bool> JaAvaliouProdutoAsync(string produtoId, string fingerprint, string tenantId)
    {
        return await _context.Avaliacoes
            .AnyAsync(a => a.ProdutoId == produtoId &&
                           a.Fingerprint == fingerprint &&
                           a.TenantId == tenantId);
    }
}
