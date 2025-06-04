using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameServer.Shared.Database;

public interface IDbContextInitializer<TContext>
    where TContext : DbContext
{
    Task InitialiseAsync();
    Task SeedAsync();
    Task TrySeedAsync();
}

public static class InitialiserExtensions
{
    internal static void AddAsyncSeeding<TContext>(this DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        builder.UseAsyncSeeding(async (context, _, ct) =>
        {
            var initialiser = serviceProvider.GetRequiredService<IDbContextInitializer<TContext>>();

            await initialiser.SeedAsync();
        });
    }

    public static async Task InitialiseDatabaseAsync<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<IDbContextInitializer<TContext>>();

        await initialiser.InitialiseAsync();
    }
}
