namespace RepCortex.Application.DTOs.Public;

public sealed record AvaliacaoPublicaResponse(
    Guid Id,
    string ProdutoId,
    int Nota,
    string Comentario,
    string? Resposta,
    DateTime DataCriacao);

public sealed record ListaAvaliacoesPublicasResponse(
    IReadOnlyList<AvaliacaoPublicaResponse> Itens,
    int Pagina,
    int TamanhoPagina,
    int Total,
    int TotalPaginas);
