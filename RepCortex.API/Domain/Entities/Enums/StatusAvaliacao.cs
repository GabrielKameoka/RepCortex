namespace RepCortex.Domain.Entities.Enums;

/// <summary>
/// Estados possíveis de uma avaliação: Pendente (revisão), Aprovada (publicada) ou Rejeitada (bloqueada).
/// </summary>
public enum StatusAvaliacao
{
    Pendente = 0,
    Aprovada = 1,
    Rejeitada = 2
}