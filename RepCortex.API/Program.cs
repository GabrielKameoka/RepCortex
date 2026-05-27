using Microsoft.EntityFrameworkCore;
using RepCortex.Domain.Interfaces;
using RepCortex.Domain.Interfaces.Service;
using RepCortex.Infrastructure.Data;
using RepCortex.Infrastructure.Repositories;
using RepCortex.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Recupera a String de Conexão do appsettings.Development.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Registra o DbContext usando o provedor do PostgreSQL (Npgsql)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. Registra os Repositórios (Camada API conhece a abstração do Domain e a implementação da Infra)
builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
builder.Services.AddScoped<IAnaliseSentimentoService, AnaliseSentimentoService>();

builder.Services.AddScoped<RepCortex.Application.UseCases.CriarAvaliacaoUseCase>();

// Configurações padrão da API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();