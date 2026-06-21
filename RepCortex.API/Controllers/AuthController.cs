using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepCortex.Application.DTOs.Auth;
using RepCortex.Application.UseCases.Auth;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly RegistrarTenantUseCase _registrarTenantUseCase;
    private readonly IIdentityService _identityService;

    public AuthController(RegistrarTenantUseCase registrarTenantUseCase, IIdentityService identityService)
    {
        _registrarTenantUseCase = registrarTenantUseCase;
        _identityService = identityService;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarTenantRequest request)
    {
        var resultado = await _registrarTenantUseCase.ExecutarAsync(request);

        if (!resultado.Sucesso)
            return BadRequest(new { mensagem = resultado.Mensagem });

        return Ok(resultado);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (sucesso, token, erro) = await _identityService.LoginAsync(request.TenantId, request.Email, request.Senha);

        if (!sucesso)
            return Unauthorized(new { mensagem = erro });

        return Ok(new { token });
    }
}