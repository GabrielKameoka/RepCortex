using RepCortex.Domain.Entities;

namespace RepCortex.Application.Interfaces;

public interface IObterTodasAvaliacoesUseCase
{
    Task<IEnumerable<Avaliacao>> ExecutarAsync();
}