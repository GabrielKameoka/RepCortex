using RepCortex.Application.DTOs;
using RepCortex.Domain.Entities;

namespace RepCortex.Application.Interfaces;

public interface ICriarAvaliacaoUseCase
{  
    Task<Avaliacao> ExecutarAsync(CriarAvaliacaoRequest request);
}