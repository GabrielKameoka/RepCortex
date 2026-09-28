using System.Threading.Tasks;
using FluentAssertions;
using RepCortex.Infrastructure.Services;
using Xunit;

namespace RepCortex.Tests;

public class AnaliseSentimentoTests
{
    private readonly AnaliseSentimentoService _service;

    public AnaliseSentimentoTests()
    {
        _service = new AnaliseSentimentoService();
    }

    [Theory]
    [InlineData("O produto é excelente, amei demais!", "Positivo")]
    [InlineData("Chegou super rápido, recomendo.", "Positivo")]
    [InlineData("Muito bom, material de qualidade.", "Positivo")]
    [InlineData("Horrível, quebrou no primeiro dia.", "Negativo")]
    [InlineData("Odiei, a caixa veio rasgada e faltou peça.", "Negativo")]
    [InlineData("Péssimo atendimento e produto ruim.", "Negativo")]
    [InlineData("Normal, nada demais.", "Neutro")]
    [InlineData("Não gostei, produto horrível", "Negativo")]
    [InlineData("Não é bom, na verdade é terrível", "Negativo")]
    public async Task AnalisarSentimentoAsync_DeveClassificarCorretamente(string texto, string resultadoEsperado)
    {
        // Act
        var resultado = await _service.AnalisarSentimentoAsync(texto);

        // Assert
        resultado.Should().Be(resultadoEsperado);
    }

    [Fact]
    public async Task AnalisarSentimentoAsync_DeveTratarNegacao()
    {
        // "bom" é positivo, mas precedido por "não" deve ser negativo
        var resultadoSemNegacao = await _service.AnalisarSentimentoAsync("Este produto é bom");
        var resultadoComNegacao = await _service.AnalisarSentimentoAsync("Este produto não é bom");

        resultadoSemNegacao.Should().Be("Positivo");
        resultadoComNegacao.Should().Be("Negativo");
    }

    [Fact]
    public async Task AnalisarSentimentoAsync_DeveTratarConjuncaoContrastiva()
    {
        // "O produto é bom, mas o atendimento foi horrível." -> O sentimento após o "mas" tem peso maior
        var resultado = await _service.AnalisarSentimentoAsync("O produto é bom, mas o atendimento foi horrível");
        resultado.Should().Be("Negativo");
    }
}
