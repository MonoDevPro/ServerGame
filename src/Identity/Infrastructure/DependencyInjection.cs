using System.Reflection;
using Infra.Notification.Interceptors;
using Infra.Services.DependencyInjection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerGame.Infrastructure.Hosted;
using ServerGame.Infrastructure.Persistence.Interceptors;

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
        
        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, NotificationInterceptor>();

        // Identity
        builder.ConfigureIdentityServices((opt, provider) =>
            opt.AddConnectionString(
                    connectionString,
                    Assembly.GetExecutingAssembly())
                .AddInterceptors(provider.GetServices<ISaveChangesInterceptor>()));
        
        // Persistence
        builder.ConfigurePersistenceServices(
            (opt, provider) => 
                opt.AddConnectionString(
                        connectionString,
                        Assembly.GetExecutingAssembly())
                    .AddInterceptors(provider.GetServices<ISaveChangesInterceptor>()));

        // Notification
        builder.ConfigureNotificationServices();
    }
}
