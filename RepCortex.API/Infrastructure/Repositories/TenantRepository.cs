using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces.Repository;
using RepCortex.Infrastructure.Data;

namespace RepCortex.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;

    public TenantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Tenant tenant)
    {
        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteSlugAsync(string id)
    {
        // O Id aqui é o próprio Slug em caixa baixa
        return await _context.Tenants.AnyAsync(t => t.Id == id.ToLower().Trim());
    }

    public async Task<Tenant?> ObterPorApiKeyAsync(string apiKey)
    {
        return await _context.Tenants.FirstOrDefaultAsync(t => t.ApiKey == apiKey);
    }
}