using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Persistence;
using Infrastructure.Repositories;
using Application.Interfaces.Services;
using Application.UseCases.Subastas.CrearSubasta;
using Application.UseCases.Subastas.ObtenerSubastas;
using Application.UseCases.Subastas.ObtenerSubastaPorId;
using Application.UseCases.Subastas.Pujar;
using Application.UseCases.Subastas.ObtenerPujas;
using Application.UseCases.Subastas.ObtenerPujasPorComprador;
using Application.UseCases.Subastas.ObtenerSubastasPorVendedor;
using Domain.Entities;
using Application.DTOs;
using Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ISubastaRepository, SubastaRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRepository<Subasta>, Repository<Subasta>>();

builder.Services.AddScoped<
    ICommandHandler<CrearSubastaCommand, SubastaDto>,
    CrearSubastaHandler>();

builder.Services.AddScoped<
    IQueryHandler<ObtenerSubastasQuery, List<SubastaDto>>,
    ObtenerSubastasHandler>();

builder.Services.AddScoped<
    IQueryHandler<ObtenerSubastaPorIdQuery, SubastaDto?>,
    ObtenerSubastaPorIdHandler>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddCors(opciones =>
    opciones.AddPolicy("frontend", politica =>
        politica
            .WithOrigins("http://localhost:5500")
            .AllowAnyHeader()
            .AllowAnyMethod()));

builder.Services.AddScoped<CrearPujaHandler>();
builder.Services.AddScoped<ObtenerPujasHandler>();
builder.Services.AddScoped<ObtenerPujasPorCompradorHandler>();
builder.Services.AddScoped<ObtenerSubastasPorVendedorHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.UseCors("frontend");

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
