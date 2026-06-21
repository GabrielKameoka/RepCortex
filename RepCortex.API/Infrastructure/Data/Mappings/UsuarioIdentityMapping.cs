using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepCortex.Infrastructure.Identity;

namespace RepCortex.Infrastructure.Data.Mappings;

public class UsuarioIdentityMapping : IEntityTypeConfiguration<UsuarioIdentity>
{
    public void Configure(EntityTypeBuilder<UsuarioIdentity> builder)
    {
        builder.Property(u => u.NomeCompleto).IsRequired().HasMaxLength(150);
        builder.Property(u => u.TenantId).IsRequired().HasMaxLength(50);

        // Remove o índice padrão de e-mail único global
        builder.DropUniqueEmailIndex();

        // Cria o índice composto: E-mail único apenas DENTRO do mesmo Tenant
        builder.HasIndex(u => new { u.NormalizedEmail, u.TenantId }).IsUnique();
    }
}

public static class IdentityMappingExtensions
{
    public static void DropUniqueEmailIndex(this EntityTypeBuilder<UsuarioIdentity> builder)
    {
        var index = builder.Metadata.GetIndexes()
            .FirstOrDefault(i =>
                i.Properties.Any(p => p.Name == nameof(UsuarioIdentity.NormalizedEmail)) && i.IsUnique &&
                i.Properties.Count == 1);

        if (index != null) builder.Metadata.RemoveIndex(index);
    }
}