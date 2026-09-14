using Application.UseCases.Subastas.ProcesarSubastas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Api.Background;

public class ProcesarSubastasWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ProcesarSubastasWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var handler = scope.ServiceProvider
                .GetRequiredService<ProcesarSubastasHandler>();

            await handler.Handle(stoppingToken);

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }
    }
}