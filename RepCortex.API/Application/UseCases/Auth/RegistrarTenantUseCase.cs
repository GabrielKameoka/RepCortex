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
    private readonly AppDbContext _context;

    public RegistrarTenantUseCase(ITenantRepository tenantRepository, IIdentityService identityService, AppDbContext context)
    {
        _tenantRepository = tenantRepository;
        _identityService = identityService;
        _context = context;
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

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 3. Cria a entidade de domínio do Tenant (Permite localhost e * por padrão em dev)
            var novoTenant = new Tenant(slugProcessado, request.NomeComercial, "localhost;*");

            // 4. Cria a entidade de domínio do Usuário Administrador
            var usuarioId = Guid.NewGuid().ToString();
            var novoUsuario = new Usuario(usuarioId, request.NomeCompletoUsuario, request.Email, novoTenant.Id);

            // 5. Persiste o Tenant primeiro para respeitar a Foreign Key do banco
            await _tenantRepository.AdicionarAsync(novoTenant);

            // 6. Delega a criação física e hash de senha para o IdentityService
            var (userSucesso, userErro, _) = await _identityService.RegistrarUsuarioAsync(novoUsuario, request.Senha);

            if (!userSucesso)
            {
                await transaction.RollbackAsync();
                return new RegistrarTenantResponse(false, userErro, null, null, null, null);
            }

            await transaction.CommitAsync();

            // 7. Auto-login: Gera o Token imediatamente após o cadastro para uma UX fluida
            var (_, token, _) = await _identityService.LoginAsync(novoTenant.Id, request.Email, request.Senha);

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
            return new RegistrarTenantResponse(false, $"Erro inesperado durante o cadastro: {ex.Message}", null, null, null, null);
        }
    }
}