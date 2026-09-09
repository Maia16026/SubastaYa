using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Persistence;
using Infrastructure.Repositories;
using Application.Interfaces.Services;
using Application.UseCases.Subastas.CrearSubasta;
using Application.UseCases.Subastas.ObtenerSubastas;
using Domain.Entities;
using Infrastructure.Seed;
using Application.DTOs;

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
    IQueryHandler<ObtenerSubastasQuery, SubastaDto?>,
    ObtenerSubastasHandler>();

// Billetera: repository y handler CQRS
builder.Services.AddScoped<IBilleteraRepository, BilleteraRepository>();
builder.Services.AddScoped<
    IQueryHandler<Application.UseCases.Billetera.ObtenerBilletera.ObtenerBilleteraQuery, Application.DTOs.BilleteraDto?>,
    Application.UseCases.Billetera.ObtenerBilletera.ObtenerBilleteraHandler>();

builder.Services.AddScoped<
    ICommandHandler<Application.UseCases.Billetera.DepositarSaldo.DepositarSaldoCommand, Application.DTOs.BilleteraDto?>,
    Application.UseCases.Billetera.DepositarSaldo.DepositarSaldoHandler>();

builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Ejecutar seed solo en Development
    await app.Services.SeedAsync();
}

// Middleware global de excepciones: mapear DomainException -> 400, otras -> 500
app.UseMiddleware<Api.Middleware.ExceptionMiddleware>();

app.UseHttpsRedirection();

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

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
