using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Application.Converters;
using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Contexts;
using R3M.Financas.Back.Repository.Data;
using R3M.Financas.Back.Repository.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Register repositories for dependency injection
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IPeriodoRepository, PeriodoRepository>();
builder.Services.AddScoped<IInstituicaoRepository, InstituicaoRepository>();
builder.Services.AddScoped<IMovimentacaoRepository, MovimentacaoRepository>();
builder.Services.AddScoped<ITipoCategoriaRepository, TipoCategoriaRepository>();

builder.Services.AddScoped<IConverter<PeriodoResponse, Periodo>, PeriodoConverter>();
builder.Services.AddScoped<IConverter<CategoriaResponse, Categoria>, CategoriaResponseConverter>();
builder.Services.AddScoped<IConverter<CategoriaRequest, Categoria>, CategoriaRequestConverter>();
builder.Services.AddScoped<IConverter<InstituicaoResponse, Instituicao>, InstituicaoResponseConverter>();
builder.Services.AddScoped<IConverter<InstituicaoRequest, Instituicao>, InstituicaoRequestConverter>();
builder.Services.AddScoped<IConverter<TipoCategoriaResponse, TipoCategoria>, TipoCategoriaConverter>();
builder.Services.AddScoped<IConverter<MovimentacaoRequest, Movimentacao>, MovimentacaoRequestConverter>();
builder.Services.AddScoped<IConverter<MovimentacaoResponse, Movimentacao>, MovimentacaoResponseConverter>();


builder.Services.AddDbContext<FinancasContext>(opt => 
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    opt.UseSnakeCaseNamingConvention();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
}

app.UseSwagger();
app.UseSwaggerUI(a =>
{
    a.SwaggerEndpoint("/swagger/v1/swagger.json", "R3M.Financas.Back.Api v1");
    a.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();

public partial class Program
{   
}