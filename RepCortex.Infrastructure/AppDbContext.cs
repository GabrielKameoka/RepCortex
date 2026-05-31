using Microsoft.EntityFrameworkCore;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenantService) : base(options)
    {
        _tenantService = tenantService; 
    }

    public DbSet<Avaliacao> Avaliacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Avaliacao>().HasQueryFilter(a => a.TenantId == _tenantService.ObterTenantId());
        // Mapeamento fluente (Boa prática ao invés de sujar a entidade com Data Annotations)
        modelBuilder.Entity<Avaliacao>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.ClienteId).IsRequired().HasMaxLength(50);
            e.Property(a => a.UsuarioIdExterno).IsRequired().HasMaxLength(100);
            e.Property(a => a.ProdutoId).IsRequired().HasMaxLength(50);
            e.Property(a => a.Comentario).HasMaxLength(1000);
            e.Property(a => a.IpOrigem).HasMaxLength(45); // Suporta IPv6
            e.Property(a => a.Status).HasConversion<string>(); // Salva o Enum como texto no banco
        });
    }
}   