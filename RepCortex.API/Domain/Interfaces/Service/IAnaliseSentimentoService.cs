namespace RepCortex.Domain.Interfaces.Service;

/// <summary>
/// Analisa sentimento de um texto via IA. Retorna "Positivo", "Negativo", "Neutro" ou "NaoAnalisado".
/// Implementação: ML.NET (local) ou serviço externo.
/// </summary>
public interface IAnaliseSentimentoService
{
    /// <summary>
    /// Analisa o texto e retorna classificação do sentimento.
    /// Nunca retorna nulo, sempre um valor válido mappável para SentimentoAvaliacao enum.
    /// </summary>
    Task<string> AnalisarSentimentoAsync(string texto);
}