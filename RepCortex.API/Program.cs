using Microsoft.EntityFrameworkCore;
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
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseRouting();

app.UseMiddleware<RepCortex.Infrastructure.Middlewares.TenantMiddleware>(); 

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

app.UseAuthorization();
app.MapControllers();

app.Run();