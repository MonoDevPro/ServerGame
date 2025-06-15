using System.Reflection;
using Infra.Services.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerGame.Infrastructure.Hosted;

namespace ServerGame.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddHostedService<DataSeedHosted>();
        
        var connectionString = builder.Configuration.GetConnectionString("serverdb");
        
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'serverdb' is not configured.");

        // Identity
        builder.ConfigureIdentityServices(
            (opt, provider) => 
                opt.AddConnectionString(
                        connectionString,
                        Assembly.GetExecutingAssembly())
                    .AddMyInterceptors(provider));
        
        // Persistence
        builder.ConfigurePersistenceServices(
            (opt, provider) => 
                opt.AddConnectionString(
                        connectionString,
                        Assembly.GetExecutingAssembly())
                    .AddMyInterceptors(provider));

        // Notification
        builder.ConfigureNotificationServices();
    }
}
