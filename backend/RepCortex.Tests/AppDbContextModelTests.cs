using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RepCortex.Application.Services;
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

        var avaliacao = context.Model.FindEntityType(typeof(Avaliacao));
        avaliacao.Should().NotBeNull();
        var nomeExterno = avaliacao!.FindProperty(nameof(Avaliacao.NomeUsuarioExterno));
        nomeExterno.Should().NotBeNull();
        nomeExterno!.IsNullable.Should().BeTrue();
        nomeExterno.GetMaxLength().Should().Be(100);
        avaliacao.FindProperty(nameof(Avaliacao.ClienteId))!.IsNullable.Should().BeTrue();
        context.Database.GetMigrations()
            .Should().Contain("20260928000100_AdicionarPoliticaModeracaoAoTenant");
        context.Database.GetMigrations()
            .Should().Contain("20260929000100_AdicionarNomeUsuarioExternoAvaliacao");
        context.Database.GetMigrations()
            .Should().Contain("20260930000100_TornarClienteIdLegadoOpcional");
    }
}
