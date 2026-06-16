using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepCortex.Domain.Entities;

namespace RepCortex.Infrastructure.Data.Mappings;

public class AvaliacaoMapping : IEntityTypeConfiguration<Avaliacao>
{
    public void Configure(EntityTypeBuilder<Avaliacao> builder)
    {
        builder.ToTable("Avaliacoes");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ProdutoId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Comentario).HasMaxLength(2000);
        builder.Property(a => a.ClienteId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.UsuarioIdExterno).IsRequired().HasMaxLength(100);
        builder.Property(a => a.IpOrigem).HasMaxLength(45);
        builder.Property(a => a.Fingerprint).HasMaxLength(255);

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Sentimento).HasConversion<string>().HasMaxLength(30);

        builder.HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}