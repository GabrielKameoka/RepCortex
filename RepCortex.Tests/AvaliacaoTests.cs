using FluentAssertions;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Entities.Enums;
using Xunit;

namespace RepCortex.Tests;

public class AvaliacaoTests
{
    [Fact]
    public void Construtor_DeveAutoAprovar_QuandoNotaFor5ESentimentoPositivo()
    {
        // Arrange
        var tenantId = "loja-teste-01";
        var clienteId = Guid.NewGuid().ToString();
        var usuarioIdExterno = "usr_123";
        var produtoId = "prod_999";
        var nota = 5;
        var comentario = "Amei o produto, excelente qualidade!";
        var ipOrigem = "127.0.0.1";
        var fingerprint = "hash_dispositivo_xyz";
        var sentimento = SentimentoAvaliacao.Positivo;

        // Act
        var avaliacao = new Avaliacao(
            tenantId,
            clienteId,
            usuarioIdExterno,
            produtoId,
            nota,
            comentario,
            ipOrigem,
            fingerprint,
            sentimento
        );

        avaliacao.Status.Should().Be(StatusAvaliacao.Aprovada);
        avaliacao.Sentimento.Should().Be(SentimentoAvaliacao.Positivo);
    }

    [Fact]
    public void Construtor_DeveReterComoPendente_QuandoNotaFor5MasSentimentoForNegativo()
    {
        // Arrange
        var nota = 5;
        var comentario = "Péssimo serviço, odeio tudo."; // Nota alta com texto ruim (Ironia)
        var sentimento = SentimentoAvaliacao.Negativo;

        // Act
        var avaliacao = new Avaliacao(
            "tenant-01", "cli-1", "usr-1", "prod-1",
            nota, comentario, "127.0.0.1", "fingerprint", sentimento
        );

        // Assert
        avaliacao.Status.Should().Be(StatusAvaliacao.Pendente); // Deve ficar retido para o lojista
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Construtor_DeveReterComoPendente_QuandoNotaForBaixa(int notaBaixa)
    {
        // Act
        var avaliacao = new Avaliacao(
            "tenant-01", "cli-1", "usr-1", "prod-1",
            notaBaixa, "Texto qualquer", "127.0.0.1", "fingerprint", SentimentoAvaliacao.Neutro
        );

        // Assert
        avaliacao.Status.Should().Be(StatusAvaliacao.Pendente);
    }

    [Fact]
    public void Construtor_DeveEstourarExcecao_QuandoNotaForInvalida()
    {
        // Arrange
        var notaInvalida = 6; // Só aceita de 1 a 5

        // Act & Assert
        Action acao = () => new Avaliacao(
            "tenant-01", "cli-1", "usr-1", "prod-1",
            notaInvalida, "Comentário", "127.0.0.1", "fingerprint", SentimentoAvaliacao.Positivo
        );

        acao.Should().Throw<ArgumentException>()
            .WithMessage("A nota deve estar entre 1 e 5.");
    }
}