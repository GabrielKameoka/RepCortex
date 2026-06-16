using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces.Service;
using RepCortex.Infrastructure.Identity;
using RepCortex.Infrastructure.Data.Mappings; // Onde estarão os mapeamentos

namespace RepCortex.Infrastructure.Data;

// Alterado para IdentityDbContext para herdar corretamente todo o ecossistema de tabelas do Identity Core
public class AppDbContext : IdentityDbContext<UsuarioIdentity>
{
    private readonly ITenantService _tenantService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }

    public string TenantId => _tenantService.ObterTenantId();

    // Tabelas do Core Business
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Avaliacao> Avaliacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Executa as configurações nativas do Identity (Crucial herdar primeiro)
        base.OnModelCreating(modelBuilder);

        // 2. Aplica as configurações isoladas da Fluent API (Clean Code)
        modelBuilder.ApplyConfiguration(new TenantMapping());
        modelBuilder.ApplyConfiguration(new AvaliacaoMapping());
        modelBuilder.ApplyConfiguration(new UsuarioIdentityMapping());

        // 3. Filtros Globais Dinâmicos para Multi-tenancy
        modelBuilder.Entity<Avaliacao>()
            .HasQueryFilter(a => string.IsNullOrEmpty(TenantId) || a.TenantId == TenantId);

        modelBuilder.Entity<UsuarioIdentity>()
            .HasQueryFilter(u => string.IsNullOrEmpty(TenantId) || u.TenantId == TenantId);
    }
}