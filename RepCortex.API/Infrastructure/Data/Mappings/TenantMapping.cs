using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepCortex.Domain.Entities;

namespace RepCortex.Infrastructure.Data.Mappings;

public class TenantMapping : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(50);
        builder.Property(t => t.NomeComercial).IsRequired().HasMaxLength(150);
        builder.Property(t => t.ApiKey).IsRequired().HasMaxLength(100);
        builder.Property(t => t.DominiosAutorizados).IsRequired().HasMaxLength(500);

        builder.HasIndex(t => t.ApiKey).IsUnique();
    }
}