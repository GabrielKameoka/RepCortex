namespace RepCortex.Application.DTOs.Dashboard;

public sealed record ResumoAvaliacoes(
    int Total, double MediaNotas, int Positivas, int Neutras,
    int Negativas, int Pendentes);

public sealed record VolumetriaDia(DateTime DiaUtc, int Quantidade);
