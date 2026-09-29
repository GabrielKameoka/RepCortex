using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RepCortex.Application.UseCases;
using RepCortex.Domain.Entities;
using RepCortex.Infrastructure.Data;
using Xunit;

namespace RepCortex.Tests;

public class AppDbContextModelTests
{
    [Fact]
    public void Modelo_DeveMapearAvaliacaoSemConectarAoBanco()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=repcortex_model_test;Username=test;Password=test")
            .Options;

        using var context = new AppDbContext(options, new TenantService());

        context.Model.FindEntityType(typeof(Avaliacao)).Should().NotBeNull();
        context.Database.GetMigrations()
            .Should().Contain("20260928000100_AdicionarPoliticaModeracaoAoTenant");
    }
}
