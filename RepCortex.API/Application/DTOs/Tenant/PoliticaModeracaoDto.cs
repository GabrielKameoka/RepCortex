using RepCortex.Domain.Entities.Enums;

namespace RepCortex.Application.DTOs.Tenant;

public sealed record PoliticaModeracaoResponse(PoliticaModeracao Politica);

public sealed record AtualizarPoliticaModeracaoRequest(
    PoliticaModeracao Politica);
