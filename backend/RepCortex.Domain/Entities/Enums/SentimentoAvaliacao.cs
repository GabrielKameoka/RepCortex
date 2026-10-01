namespace RepCortex.Domain.Entities.Enums;

/// <summary>
/// Classificação do sentimento do comentário: NaoAnalisado, Positivo, Neutro ou Negativo.
/// Preenchido pela IA (ML.NET).
/// </summary>
public enum SentimentoAvaliacao
{
    NaoAnalisado = 0,
    Positivo = 1,
    Neutro = 2,
    Negativo = 3
}