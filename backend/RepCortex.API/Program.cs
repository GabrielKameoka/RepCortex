using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RepCortex.API.Hubs;
using RepCortex.API.Security;
using RepCortex.Application.Services;
using RepCortex.Application.Abstractions.Services;
using RepCortex.Infrastructure.Data;
using RepCortex.Infrastructure.Repositories;
using RepCortex.Infrastructure.Services;
using RepCortex.Application.UseCases.Auth;
using RepCortex.Application.Abstractions.Persistence;
using RepCortex.Infrastructure.Identity;
using RepCortex.Infrastructure.Security;
using RepCortex.Application.Abstractions.Context;
using RepCortex.API;
using Scalar.AspNetCore;

DotNetEnv.Env.Load(); // Carrega o arquivo .env para o ambiente antes de subir a API


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Validação limpa
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException(
        "Configure a variável de ambiente 'Jwt__Secret' com uma chave JWT válida antes de inicializar a API.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "Configure a variável de ambiente 'Jwt__Issuer' com um emissor JWT válido antes de inicializar a API.");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Configure a variável de ambiente 'Jwt__Audience' com uma audience JWT válida antes de inicializar a API.");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure a ConnectionStrings__DefaultConnection válida antes de inicializar a API.");
}

builder.Services.AddIdentityCore<UsuarioIdentity>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>();

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
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IRequestContext, RequestContext>();

// --- Serviços de Infraestrutura ---
builder.Services
    .AddSingleton<IAnaliseSentimentoService,
        AnaliseSentimentoService>(); // Mantido Singleton para carregar o modelo ML.NET uma única vez na memória
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// --- Serviços de Contexto Híbrido ---
builder.Services.AddScoped<ITenantService, TenantService>();

// --- Serviços de Aplicação ---
builder.Services.AddScoped<RepCortex.Application.Services.AvaliacaoService>();
builder.Services.AddScoped<RepCortex.Application.Services.DashboardService>();
builder.Services.AddScoped<IDashboardEventPublisher, DashboardEventPublisher>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<RepCortex.Application.Services.TenantSettingsService>();
builder.Services.AddScoped<RegistrarTenantUseCase>();
builder.Services.AddScoped<LoginUseCase>();

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
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // Verifica se a requisição está indo em direção ao seu Hub mapeado
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/dashboard"))
                {
                    // Injeta o token recuperado da URL diretamente no contexto da requisição
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
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
            mensagem =
                "Muitas requisições enviadas. Limite de taxa excedido para o seu Tenant/IP. Tente novamente em breve."
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

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:4200", "http://127.0.0.1:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicies.Dashboard, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
    options.AddPolicy(CorsPolicies.PublicApi, policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var publicUrl = builder.Configuration["OpenApi:PublicUrl"];
        if (!string.IsNullOrWhiteSpace(publicUrl))
            document.Servers = [new() { Url = publicUrl }];
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "RepCortex API";
    options.Theme = ScalarTheme.Purple;
    // Usando o padrão relativo nativo do .NET 9
    options.OpenApiRoutePattern = "/openapi/v1.json";
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseMiddleware<RepCortex.API.Middlewares.TenantMiddleware>();


app.MapControllers();
app.MapHub<DashboardHub>("/hubs/dashboard").RequireCors(CorsPolicies.Dashboard);

var applyMigrations = app.Environment.IsDevelopment() ||
                      builder.Configuration.GetValue<bool>("Database:ApplyMigrations");

if (applyMigrations)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (builder.Configuration.GetValue<bool>("Demo:SeedData"))
{
    using var scope = app.Services.CreateScope();
    await RepCortex.Infrastructure.Seeding.DemoSeeder.SeedAsync(
        scope.ServiceProvider,
        app.Logger,
        app.Lifetime.ApplicationStopping);
}

app.Run();
