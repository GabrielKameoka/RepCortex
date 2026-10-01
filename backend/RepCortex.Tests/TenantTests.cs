using FluentAssertions;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Entities.Enums;
using System;
using Xunit;

namespace RepCortex.Tests;

public class TenantTests
{
    [Fact]
    public void Tenant_DeveIniciarComPoliticaAutomatica()
    {
        var tenant = new Tenant("loja", "Loja");

        tenant.PoliticaModeracao.Should().Be(PoliticaModeracao.Automatica);
    }

    [Fact]
    public void Tenant_DevePermitirAlterarPoliticaDeModeracao()
    {
        var tenant = new Tenant("loja", "Loja");

        tenant.DefinirPoliticaModeracao(PoliticaModeracao.Manual);

        tenant.PoliticaModeracao.Should().Be(PoliticaModeracao.Manual);
    }
    [Fact]
    public void Construtor_DeveGerarChavesPublicaESecretaCorretamente()
    {
        // Arrange
        var id = "loja-teste-02";
        var nomeComercial = "Loja de Teste Inc";

        // Act
        var tenant = new Tenant(id, nomeComercial);

        // Assert
        tenant.Id.Should().Be("loja-teste-02");
        tenant.NomeComercial.Should().Be("Loja de Teste Inc");
        tenant.PublishableKey.Should().StartWith("rc_pub_");
        tenant.SecretKey.Should().StartWith("rc_sec_");
        tenant.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Construtor_DeveNormalizarSlugDoId()
    {
        // Arrange
        var id = "  Loja Teste 123  ";
        var nomeComercial = "Loja de Teste Inc";

        // Act
        var tenant = new Tenant(id, nomeComercial);

        // Assert
        tenant.Id.Should().Be("loja-teste-123");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Construtor_DeveLancarExcecao_QuandoIdForInvalido(string? idInvalido)
    {
        // Act & Assert
        Action acao = () => new Tenant(idInvalido!, "Nome");
        acao.Should().Throw<ArgumentException>();
    }
}
