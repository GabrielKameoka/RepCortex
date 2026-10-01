using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Entities.Enums;
using RepCortex.Application.Abstractions.Persistence;
using RepCortex.Infrastructure.Data;

namespace RepCortex.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache;

    public TenantRepository(AppDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
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

    public async Task<Tenant?> ObterPorIdAsync(string id)
    {
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AtualizarAsync(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync($"tenant:pubkey:{tenant.PublishableKey}");
        await _cache.RemoveAsync($"tenant:seckey:{tenant.SecretKey}");
    }

    public async Task<Tenant?> ObterPorPublishableKeyAsync(string publishableKey)
    {
        var cacheKey = $"tenant:pubkey:{publishableKey}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            try
            {
                return JsonSerializer.Deserialize<TenantCache>(cachedData)?.ToDomain();
            }
            catch
            {
                // Fallback silencioso para o banco se houver falha de desserialização
            }
        }

        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.PublishableKey == publishableKey);
        if (tenant != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(TenantCache.FromDomain(tenant)), options);
        }

        return tenant;
    }

    public async Task<Tenant?> ObterPorSecretKeyAsync(string secretKey)
    {
        var cacheKey = $"tenant:seckey:{secretKey}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            try
            {
                return JsonSerializer.Deserialize<TenantCache>(cachedData)?.ToDomain();
            }
            catch
            {
                // Fallback silencioso para o banco se houver falha de desserialização
            }
        }

        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.SecretKey == secretKey);
        if (tenant != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(TenantCache.FromDomain(tenant)), options);
        }

        return tenant;
    }

    private sealed record TenantCache(string Id, string NomeComercial, string PublishableKey,
        string SecretKey, string DominiosAutorizados, bool Ativo,
        PoliticaModeracao PoliticaModeracao, DateTime DataCriacao)
    {
        public static TenantCache FromDomain(Tenant tenant) => new(
            tenant.Id, tenant.NomeComercial, tenant.PublishableKey, tenant.SecretKey,
            tenant.DominiosAutorizados, tenant.Ativo, tenant.PoliticaModeracao, tenant.DataCriacao);

        public Tenant ToDomain() => Tenant.Restaurar(Id, NomeComercial, PublishableKey,
            SecretKey, DominiosAutorizados, Ativo, PoliticaModeracao, DataCriacao);
    }
}
