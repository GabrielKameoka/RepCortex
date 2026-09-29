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

    [Fact]
    public void Construtor_DeveReterComoPendente_QuandoPoliticaForManual()
    {
        var avaliacao = new Avaliacao(
            "tenant-01", "cli-1", "usr-1", "prod-1",
            5, "Excelente produto", "127.0.0.1", "fingerprint",
            SentimentoAvaliacao.Positivo, PoliticaModeracao.Manual);

        avaliacao.Status.Should().Be(StatusAvaliacao.Pendente);
    }

    [Fact]
    public void Construtor_DeveGuardarNomeExternoNormalizado()
    {
        var avaliacao = new Avaliacao(
            "tenant-01", "cli-1", "usr-1", "prod-1", 5,
            "Excelente produto", "127.0.0.1", "fingerprint",
            SentimentoAvaliacao.Positivo, nomeUsuarioExterno: "  Mariana Silva  ");

        avaliacao.NomeUsuarioExterno.Should().Be("Mariana Silva");
        avaliacao.UsuarioIdExterno.Should().Be("usr-1");
        avaliacao.ClienteId.Should().Be("cli-1");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Construtor_DeveAceitarAvaliacaoSemNomeExterno(string? nome)
    {
        var avaliacao = new Avaliacao(
            "tenant-01", "cli-1", "usr-1", "prod-1", 5,
            "Excelente produto", "127.0.0.1", "fingerprint",
            SentimentoAvaliacao.Positivo, nomeUsuarioExterno: nome);

        avaliacao.NomeUsuarioExterno.Should().BeNull();
    }

    [Fact]
    public void Construtor_DeveRejeitarNomeExternoAcimaDoLimite()
    {
        Action criar = () => new Avaliacao(
            "tenant-01", "cli-1", "usr-1", "prod-1", 5,
            "Excelente produto", "127.0.0.1", "fingerprint",
            SentimentoAvaliacao.Positivo, nomeUsuarioExterno: new string('A', 101));

        criar.Should().Throw<ArgumentException>()
            .WithMessage("O nome do usuário externo deve ter até 100 caracteres.");
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

    [Fact]
    public void Responder_DeveAutoAprovar_QuandoEstiverPendente()
    {
        // Arrange
        var avaliacao = new Avaliacao(
            "tenant-01", "cli-1", "usr-1", "prod-1",
            2, "Produto ruim", "127.0.0.1", "fingerprint", SentimentoAvaliacao.Negativo
        );
        avaliacao.Status.Should().Be(StatusAvaliacao.Pendente);

        // Act
        avaliacao.Responder("Lamento o ocorrido, vamos te enviar um produto novo!");

        // Assert
        avaliacao.Status.Should().Be(StatusAvaliacao.Aprovada);
        avaliacao.Resposta.Should().Be("Lamento o ocorrido, vamos te enviar um produto novo!");
    }
}
