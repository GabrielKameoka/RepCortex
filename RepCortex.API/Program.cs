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

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Validação limpa: Só barra se você esquecer de passar o valor
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("Configure a variável de ambiente 'Jwt__Secret' antes de inicializar a API.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException("Configure a variável de ambiente 'Jwt__Issuer' antes de inicializar a API.");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("Configure a variável de ambiente 'Jwt__Audience' antes de inicializar a API.");
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

// --- Cache Distribuído (Redis) ---
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "RepCortex:";
});

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

// --- Rate Limiting ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        var respostaErro = new
        {
            mensagem = "Muitas requisições enviadas. Limite de taxa excedido para o seu Tenant/IP. Tente novamente em breve."
        };
        await context.HttpContext.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(respostaErro), token);
    };

    options.AddPolicy("PublicWidgetPolicy", httpContext =>
    {
        var tenantId = httpContext.User.FindFirstValue(AuthClaimTypes.TenantId);

        if (string.IsNullOrEmpty(tenantId))
        {
            var apiKeyHeader = httpContext.Request.Headers["X-Api-Key"].ToString();
            if (!string.IsNullOrEmpty(apiKeyHeader))
            {
                tenantId = apiKeyHeader;
            }
        }

        tenantId ??= "anonymous";
        var ipOrigem = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
        var partitionKey = $"rate_limit_tenant:{tenantId}:ip:{ipOrigem}";

        return System.Threading.RateLimiting.RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => new System.Threading.RateLimiting.SlidingWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 3,
                QueueLimit = 0
            });
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseRouting();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "RepCortex API";
    options.Theme = ScalarTheme.Purple;
    options.OpenApiRoutePattern = "/openapi/v1.json";
});

app.UseAuthentication(); 

app.UseMiddleware<RepCortex.Infrastructure.Middlewares.TenantMiddleware>(); 
app.UseRateLimiter();    
app.UseAuthorization();  

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); 
}

app.Run();