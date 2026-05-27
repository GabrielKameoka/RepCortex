namespace RepCortex.Domain.Interfaces.Service;

public interface IAnaliseSentimentoService
{
    /// <summary>
    /// Analisa o texto e retorna se o sentimento é "Positivo", "Negativo" ou "Neutro"
    /// </summary>
    Task<string> AnalisarSentimentoAsync(string texto);
}