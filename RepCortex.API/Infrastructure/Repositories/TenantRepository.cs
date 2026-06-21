using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces.Repository;
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

        // Escrita proativa em cache
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        await _cache.SetStringAsync($"tenant:pubkey:{tenant.PublishableKey}", JsonSerializer.Serialize(tenant), cacheOptions);
        await _cache.SetStringAsync($"tenant:seckey:{tenant.SecretKey}", JsonSerializer.Serialize(tenant), cacheOptions);
    }

    public async Task<bool> ExisteSlugAsync(string id)
    {
        // O Id aqui é o próprio Slug em caixa baixa
        return await _context.Tenants.AnyAsync(t => t.Id == id.ToLower().Trim());
    }

    public async Task<Tenant?> ObterPorPublishableKeyAsync(string publishableKey)
    {
        var cacheKey = $"tenant:pubkey:{publishableKey}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            try
            {
                return JsonSerializer.Deserialize<Tenant>(cachedData);
            }
            catch
            {
                // Fallback silencioso para o banco se houver falha de desserialização
            }
        }

        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.PublishableKey == publishableKey);
        if (tenant != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tenant), options);
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
                return JsonSerializer.Deserialize<Tenant>(cachedData);
            }
            catch
            {
                // Fallback silencioso para o banco se houver falha de desserialização
            }
        }

        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.SecretKey == secretKey);
        if (tenant != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tenant), options);
        }

        return tenant;
    }
}