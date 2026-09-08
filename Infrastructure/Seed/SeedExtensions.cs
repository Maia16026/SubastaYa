using System;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Seed;

public static class SeedExtensions
{
    // Extension sobre IServiceProvider para evitar depender de tipos de ASP.NET Core en este proyecto
    public static async Task SeedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seeder = new DatabaseSeeder();
        await seeder.SeedAsync(ctx);
    }
}
