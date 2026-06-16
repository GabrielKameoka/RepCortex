namespace RepCortex.Application.DTOs.Auth;

public record RegistrarTenantRequest(
    string TenantIdSlug, // ex: "comunidade-dotnet"
    string NomeComercial, // ex: "Comunidade .NET São Paulo"
    string NomeCompletoUsuario,
    string Email,
    string Senha
);

public record RegistrarTenantResponse(
    bool Sucesso,
    string? Mensagem,
    string? TenantId,
    string? TokenJWT,
    string? ApiKey
);