using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServerGame.Infrastructure.Hosted;

public class DataSeedHosted(
    IServiceProvider provider,
    ILogger<DataSeedHosted> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (Environment.GetEnvironmentVariable("SkipNSwag") == "False")
            return;
        
        using var scope = provider.CreateScope();
        
        var contexts = scope.ServiceProvider.GetServices<DbContext>();
        
        foreach (var context in contexts)
            try
            {
                await context.Database.MigrateAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initialising the database with {SeederType}", context.GetType().Name);
                throw;
            }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Nothing to do on stop
        return Task.CompletedTask;
    }
}

