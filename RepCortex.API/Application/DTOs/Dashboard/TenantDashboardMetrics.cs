namespace RepCortex.API.Application.DTOs.Dashboard;

public record TenantDashboardMetrics(
    int TotalAvaliacoes,
    double MediaNotas,
    int TotalPositivas,
    int TotalNeutras,
    int TotalNegativas,
    int TotalPendentesModeracao,
    List<GraficoLinhaPonto> VolumetriaUltimosDias
);

public record GraficoLinhaPonto(string Data, int Quantidade);