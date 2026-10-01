using RepCortex.Application.Abstractions.Services;
using RepCortex.Application.DTOs.Auth;

namespace RepCortex.Application.UseCases.Auth;

public sealed class LoginUseCase(IIdentityService identityService)
{
    public Task<(bool Sucesso, string? Token, string? Erro)> ExecutarAsync(LoginRequest request) =>
        identityService.LoginAsync(request.TenantId, request.Email, request.Senha);
}
