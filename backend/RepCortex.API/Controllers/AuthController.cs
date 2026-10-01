using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RepCortex.Application.DTOs.Auth;
using RepCortex.Application.UseCases.Auth;
using RepCortex.API.Security;

namespace RepCortex.API.Controllers;

[ApiController]
[EnableCors(CorsPolicies.Dashboard)]
[Route("api/auth")]
[AllowAnonymous] // Mantido: Permite registrar e logar publicamente para gerar as credenciais
public class AuthController : ControllerBase
{
    private readonly RegistrarTenantUseCase _registrarTenantUseCase;
    private readonly LoginUseCase _loginUseCase;

    public AuthController(RegistrarTenantUseCase registrarTenantUseCase, LoginUseCase loginUseCase)
    {
        _registrarTenantUseCase = registrarTenantUseCase;
        _loginUseCase = loginUseCase;
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
        var (sucesso, token, erro) = await _loginUseCase.ExecutarAsync(request);

        if (!sucesso)
            return Unauthorized(new { mensagem = erro });

        return Ok(new { token });
    }
}
