using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Entities.Enums;
using RepCortex.Infrastructure.Data;
using RepCortex.Infrastructure.Identity;

namespace RepCortex.Infrastructure.Seeding;

public static class DemoSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<UsuarioIdentity>>();

        var tenantId = configuration["Demo:TenantId"]?.Trim().ToLowerInvariant();
        var adminEmail = configuration["Demo:AdminEmail"]?.Trim();
        var adminPassword = configuration["Demo:AdminPassword"];

        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Demo:TenantId, Demo:AdminEmail e Demo:AdminPassword são obrigatórios quando Demo:SeedData=true.");
        }

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            tenant = new Tenant(tenantId, "Espaço Sandbox de Testes", "localhost;127.0.0.1");
            await db.Tenants.AddAsync(tenant, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Tenant de demonstração criado: {TenantId}", tenant.Id);
        }

        var admin = await userManager.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == userManager.NormalizeEmail(adminEmail), cancellationToken);

        if (admin is null)
        {
            admin = new UsuarioIdentity
            {
                Id = Guid.NewGuid().ToString(),
                NomeCompleto = "Administrador da demonstração",
                Email = adminEmail,
                UserName = adminEmail,
                TenantId = tenant.Id,
                DataCadastro = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Não foi possível criar o administrador da demonstração: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }

            logger.LogInformation("Administrador de demonstração criado para {TenantId}.", tenant.Id);
        }

        var hasReviews = await db.Avaliacoes
            .IgnoreQueryFilters()
            .AnyAsync(a => a.TenantId == tenant.Id, cancellationToken);

        if (!hasReviews)
        {
            var reviews = new List<Avaliacao>
            {
                new(tenant.Id, "usr_1", "prod_celular", 5,
                    "Sensacional! O celular é extremamente rápido e a bateria dura dois dias inteiros. Recomendo demais!",
                    "127.0.0.1", "demo-fp-1", SentimentoAvaliacao.Positivo, nomeUsuarioExterno: "Mariana"),
                new(tenant.Id, "usr_2", "prod_fone", 4,
                    "Muito bom, material de ótima qualidade e som limpo, mas demorou um pouco para chegar.",
                    "127.0.0.1", "demo-fp-2", SentimentoAvaliacao.Positivo, nomeUsuarioExterno: "Lucas"),
                new(tenant.Id, "usr_3", "prod_relogio", 3,
                    "É ok, bonito, mas as funções são meio básicas. Pelo preço, vale a pena.",
                    "127.0.0.1", "demo-fp-3", SentimentoAvaliacao.Neutro, nomeUsuarioExterno: "Camila"),
                new(tenant.Id, "usr_4", "prod_capinha", 1,
                    "Péssimo produto! Quebrou no primeiro dia de uso e o atendimento foi horrível.",
                    "127.0.0.1", "demo-fp-4", SentimentoAvaliacao.Negativo, nomeUsuarioExterno: "Rafael")
            };

            await db.Avaliacoes.AddRangeAsync(reviews, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Avaliações fictícias de demonstração criadas para {TenantId}.", tenant.Id);
        }
    }
}
