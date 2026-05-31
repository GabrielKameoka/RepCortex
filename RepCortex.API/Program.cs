using Microsoft.EntityFrameworkCore;
using RepCortex.Application.UseCases;
using RepCortex.Domain.Interfaces;
using RepCortex.Domain.Interfaces.Service;
using RepCortex.Infrastructure.Data;
using RepCortex.Infrastructure.Repositories;
using RepCortex.Infrastructure.Services;
using Microsoft.OpenApi;
using RepCortex.Application.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registra os Repositórios (Camada API conhece a abstração do Domain e a implementação da Infra)
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddSingleton<IAnaliseSentimentoService, AnaliseSentimentoService>();
builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();

// Registra UseCases da Application
builder.Services.AddScoped<ICriarAvaliacaoUseCase, CriarAvaliacaoUseCase>();
builder.Services.AddScoped<IObterTodasAvaliacoesUseCase, ObterTodasAvaliacoesUseCase>();

// Configurações padrão da API
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseMiddleware<RepCortex.Infrastructure.Middlewares.TenantMiddleware>();

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

app.UseAuthorization();
app.MapControllers();

app.Run();