using RepCortex.Domain.Entities;
using Xunit;

namespace Verity.UnitTests.Domain;

public class AvaliacaoTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void NaoDevePermitirNotaInvalida(int notaInvalida)
    {
        // Arrange & Act
        var acao = () => new Avaliacao("cli-123", "usr-456", "prod-789", notaInvalida, "Bom", "127.0.0.1", "fp-123");

        // Assert
        Assert.Throws<ArgumentException>(acao);
    }

    [Fact]
    public void DeveCriarAvaliacaoComStatusPendente()
    {
        var avaliacao = new Avaliacao("cli-123", "usr-456", "prod-789", 5, "Excelente", "127.0.0.1", "fp-123");

        Assert.Equal(StatusAvaliacao.Pendente, avaliacao.Status);
    }
}