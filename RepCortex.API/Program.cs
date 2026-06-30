using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RepCortex.API.Hubs;
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
    throw new InvalidOperationException(
        "Configure a variável de ambiente 'Jwt__Secret' com uma chave JWT válida antes de inicializar a API.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer) || jwtIssuer.Contains("SUA_ISSUER_JWT"))
{
    throw new InvalidOperationException(
        "Configure a variável de ambiente 'Jwt__Issuer' com um emissor JWT válido antes de inicializar a API.");
}

if (string.IsNullOrWhiteSpace(jwtAudience) || jwtAudience.Contains("SUA_AUDIENCE_JWT"))
{
    throw new InvalidOperationException(
        "Configure a variável de ambiente 'Jwt__Audience' com uma audience JWT válida antes de inicializar a API.");
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
builder.Services
    .AddSingleton<IAnaliseSentimentoService,
        AnaliseSentimentoService>(); // Mantido Singleton para carregar o modelo ML.NET uma única vez na memória
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
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowFrontend");

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
app.UseMiddleware<
    RepCortex.Infrastructure.Middlewares.TenantMiddleware>(); // 2. Captura as Claims do User e define o TenantId global
app.UseRateLimiter(); // 2.5 Limitador de taxa baseado no Tenant autenticado
app.UseAuthorization(); // 3. Valida se a política (Admin, Public, Secret) bate com o endpoint
app.MapControllers();
app.MapHub<DashboardHub>("/hubs/dashboard");

// --- Aplicação automática de Migrations e Seeding para Usabilidade Out-Of-The-Box ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<UsuarioIdentity>>();
    
    try
    {
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✅ Banco de dados sincronizado e migrations aplicadas com sucesso.");
        
        // Seeding do Tenant de Teste/Sandbox
        var tenantId = "teste";
        var tenantExistente = await dbContext.Tenants.AnyAsync(t => t.Id == tenantId);
        if (!tenantExistente)
        {
            var tenant = new RepCortex.Domain.Entities.Tenant(tenantId, "Espaço Sandbox de Testes", "localhost;*");
            
            // Força as chaves padrão que o dashboard espera usando Reflection
            typeof(RepCortex.Domain.Entities.Tenant).GetProperty(nameof(RepCortex.Domain.Entities.Tenant.PublishableKey))?.SetValue(tenant, "rc_pub_809cc0f890694489a19fc72ffee99f36");
            typeof(RepCortex.Domain.Entities.Tenant).GetProperty(nameof(RepCortex.Domain.Entities.Tenant.SecretKey))?.SetValue(tenant, "rc_sec_809cc0f890694489a19fc72ffee99f36");
            
            await dbContext.Tenants.AddAsync(tenant);
            await dbContext.SaveChangesAsync();
            Console.WriteLine("🌱 Tenant Sandbox semeado com sucesso.");
        }
        
        // Seeding do Administrador do Sandbox
        var adminEmail = "admin@sandbox.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var adminIdentity = new UsuarioIdentity
            {
                Id = Guid.NewGuid().ToString(),
                NomeCompleto = "Administrador do Sandbox",
                Email = adminEmail,
                UserName = adminEmail,
                TenantId = tenantId,
                DataCadastro = DateTime.UtcNow
            };
            
            var result = await userManager.CreateAsync(adminIdentity, "Admin123!");
            if (result.Succeeded)
            {
                Console.WriteLine("🌱 Administrador do Sandbox semeado com sucesso. (Login: admin@sandbox.com / Senha: Admin123!)");
            }
            else
            {
                Console.WriteLine($"⚠️ Falha ao semear administrador: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        
        // Seeding de Avaliações Iniciais para deixar o Dashboard lindo no primeiro boot!
        var temAvaliacoes = await dbContext.Avaliacoes.AnyAsync(a => a.TenantId == tenantId);
        if (!temAvaliacoes)
        {
            var avaliacoesMock = new List<RepCortex.Domain.Entities.Avaliacao>
            {
                new(tenantId, "cli_1", "usr_1", "prod_celular", 5, "Sensacional! O celular é extremamente rápido e a bateria dura dois dias inteiros. Recomendo demais!", "127.0.0.1", "fp_1", RepCortex.Domain.Entities.Enums.SentimentoAvaliacao.Positivo),
                new(tenantId, "cli_2", "usr_2", "prod_fone", 4, "Muito bom, material de ótima qualidade e som limpo, mas demorou um pouco para chegar.", "127.0.0.1", "fp_2", RepCortex.Domain.Entities.Enums.SentimentoAvaliacao.Positivo),
                new(tenantId, "cli_3", "usr_3", "prod_relogio", 3, "É ok, bonito, mas as funções são meio básicas. Pelo preço, vale a pena.", "127.0.0.1", "fp_3", RepCortex.Domain.Entities.Enums.SentimentoAvaliacao.Neutro),
                new(tenantId, "cli_4", "usr_4", "prod_capinha", 1, "Péssimo produto! Quebrou no primeiro dia de uso e o atendimento foi horrível.", "127.0.0.1", "fp_4", RepCortex.Domain.Entities.Enums.SentimentoAvaliacao.Negativo),
                new(tenantId, "cli_5", "usr_5", "prod_carregador", 2, "Lento para carregar, esquenta demais e não veio o cabo descrito na caixa. Decepcionado.", "127.0.0.1", "fp_5", RepCortex.Domain.Entities.Enums.SentimentoAvaliacao.Negativo),
                new(tenantId, "cli_6", "usr_6", "prod_mouse", 4, "Design ergonômico excelente, perfeito para trabalhar! Porém o preço é um pouco caro.", "127.0.0.1", "fp_6", RepCortex.Domain.Entities.Enums.SentimentoAvaliacao.Positivo)
            };
            
            await dbContext.Avaliacoes.AddRangeAsync(avaliacoesMock);
            await dbContext.SaveChangesAsync();
            Console.WriteLine("🌱 Avaliações mock semeadas com sucesso.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Erro ao sincronizar ou semear o banco de dados: {ex.Message}");
    }
}

app.Run();