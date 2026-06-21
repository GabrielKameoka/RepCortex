using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RepCortex.Application.UseCases;
using RepCortex.Domain.Interfaces.Service;
using RepCortex.Infrastructure.Data;
using RepCortex.Infrastructure.Repositories;
using RepCortex.Infrastructure.Services;
using RepCortex.Application.UseCases.Auth;
using RepCortex.Domain.Interfaces.Repository;
using RepCortex.Infrastructure.Identity;
using RepCortex.Infrastructure.Security;
using Scalar.AspNetCore;
DotNetEnv.Env.Load(); // Carrega o arquivo .env para o ambiente antes de subir a API

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Contains("SUA_CHAVE_JWT"))
{
    throw new InvalidOperationException("Configure a variável de ambiente 'Jwt__Secret' com uma chave JWT válida antes de inicializar a API.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer) || jwtIssuer.Contains("SUA_ISSUER_JWT"))
{
    throw new InvalidOperationException("Configure a variável de ambiente 'Jwt__Issuer' com um emissor JWT válido antes de inicializar a API.");
}

if (string.IsNullOrWhiteSpace(jwtAudience) || jwtAudience.Contains("SUA_AUDIENCE_JWT"))
{
    throw new InvalidOperationException("Configure a variável de ambiente 'Jwt__Audience' com uma audience JWT válida antes de inicializar a API.");
}

builder.Services.AddIdentityCore<UsuarioIdentity>(options =>
    {
        // Aqui você pode customizar regras de senha se quiser (exemplo):
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>(); // Diz para o Identity salvar os dados no seu contexto do EF Core

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Repositórios ---
builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// --- Serviços de Infraestrutura ---
builder.Services.AddSingleton<IAnaliseSentimentoService, AnaliseSentimentoService>(); // Mantido Singleton para carregar o modelo ML.NET uma única vez na memória
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// --- Serviços de Contexto Híbrido ---
builder.Services.AddScoped<ITenantService, TenantService>(); 

// --- Serviços de Aplicação ---
builder.Services.AddScoped<RepCortex.Application.Services.AvaliacaoService>();
builder.Services.AddScoped<RegistrarTenantUseCase>();

// Configurações padrão da API
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = AuthSchemes.AdminJwt;
        options.DefaultChallengeScheme = AuthSchemes.AdminJwt;
    })
    .AddJwtBearer(AuthSchemes.AdminJwt, options =>
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            NameClaimType = ClaimTypes.Name
        };
    })
    .AddScheme<TenantApiKeyAuthenticationOptions, TenantApiKeyAuthenticationHandler>(
        AuthSchemes.PublishableKey,
        options => options.KeyType = TenantApiKeyType.Publishable)
    .AddScheme<TenantApiKeyAuthenticationOptions, TenantApiKeyAuthenticationHandler>(
        AuthSchemes.SecretKey,
        options => options.KeyType = TenantApiKeyType.Secret);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.AdminOnly, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.AdminJwt);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthClaimTypes.AccessType, AuthAccessTypes.Admin);
    });

    options.AddPolicy(AuthPolicies.PublicIngestOnly, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.PublishableKey);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthClaimTypes.AccessType, AuthAccessTypes.Publishable);
    });

    options.AddPolicy(AuthPolicies.SecretIntegrationOnly, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.SecretKey);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthClaimTypes.AccessType, AuthAccessTypes.Secret);
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
    app.MapScalarApiReference(options => 
    {
        options.Title = "RepCortex API";
        options.Theme = ScalarTheme.Purple;
        options.OpenApiRoutePattern = "/openapi/v1.json";
    });
}

app.UseAuthentication(); // 1. Decodifica o JWT ou valida a API Key e monta o context.User
app.UseMiddleware<RepCortex.Infrastructure.Middlewares.TenantMiddleware>(); // 2. Captura as Claims do User e define o TenantId global
app.UseAuthorization();  // 3. Valida se a política (Admin, Public, Secret) bate com o endpoint
app.MapControllers();

app.Run();