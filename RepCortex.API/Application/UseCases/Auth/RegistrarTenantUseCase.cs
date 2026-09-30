using System.Threading.Tasks;
using RepCortex.Application.DTOs.Auth;
using RepCortex.Domain.Entities;
using RepCortex.Domain.Interfaces.Repository;
using RepCortex.Domain.Interfaces.Service;
using RepCortex.Infrastructure.Data;

namespace RepCortex.Application.UseCases.Auth;

public class RegistrarTenantUseCase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IIdentityService _identityService;
    private readonly ITenantService _tenantService;
    private readonly AppDbContext _context;
    private readonly ILogger<RegistrarTenantUseCase> _logger;

    public RegistrarTenantUseCase(ITenantRepository tenantRepository, IIdentityService identityService, ITenantService tenantService, AppDbContext context, ILogger<RegistrarTenantUseCase> logger)
    {
        _tenantRepository = tenantRepository;
        _identityService = identityService;
        _tenantService = tenantService;
        _context = context;
        _logger = logger;
    }

    public async Task<RegistrarTenantResponse> ExecutarAsync(RegistrarTenantRequest request)
    {
        // 1. Processa e higieniza o Slug do Tenant
        var slugProcessado = request.TenantIdSlug.ToLower().Trim().Replace(" ", "-");

        // 2. Valida unicidade do inquilino
        var jaExiste = await _tenantRepository.ExisteSlugAsync(slugProcessado);
        if (jaExiste)
        {
            return new RegistrarTenantResponse(false, "Este identificador de espaço (Slug) já está em uso.", null, null, null, null);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 3. Cria o Tenant com uma origem local segura até o administrador configurar o domínio real.
            var novoTenant = new Tenant(slugProcessado, request.NomeComercial, "localhost;127.0.0.1");

            // 4. Cria a entidade de domínio do Usuário Administrador
            var usuarioId = Guid.NewGuid().ToString();
            var novoUsuario = new Usuario(usuarioId, request.NomeCompletoUsuario, request.Email, novoTenant.Id);

            // 5. Persiste o Tenant primeiro para respeitar a Foreign Key do banco
            await _tenantRepository.AdicionarAsync(novoTenant);

            // 6. Delega a criação física e hash de senha para o IdentityService
            _tenantService.DefinirTenantId(novoTenant.Id);
            var (userSucesso, userErro, _) = await _identityService.RegistrarUsuarioAsync(novoUsuario, request.Senha);

            if (!userSucesso)
            {
                await transaction.RollbackAsync();
                return new RegistrarTenantResponse(false, userErro, null, null, null, null);
            }

            // 7. Gera o token antes do commit para que uma falha não deixe um cadastro parcial.
            var (loginSucesso, token, loginErro) = await _identityService.LoginAsync(novoTenant.Id, request.Email, request.Senha);
            if (!loginSucesso || string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException(loginErro ?? "Não foi possível gerar a sessão do novo usuário.");

            await transaction.CommitAsync();

            return new RegistrarTenantResponse(
                true,
                "Espaço comunitário e administrador registrados com sucesso!",
                novoTenant.Id,
                token,
                novoTenant.PublishableKey,
                novoTenant.SecretKey
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Falha ao registrar o tenant {TenantId}", slugProcessado);
            return new RegistrarTenantResponse(false, "Não foi possível concluir o cadastro. Tente novamente.", null, null, null, null);
        }
    }
}
