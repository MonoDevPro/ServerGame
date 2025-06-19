using System.Reflection;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces.Identity;
using GameServer.Application.Common.Interfaces.Notification.Dispatchers;
using GameServer.Domain.Constants;
using GameServer.Domain.Entities;
using GameServer.Infrastructure.Common.Hosted;
using GameServer.Infrastructure.Identity;
using GameServer.Infrastructure.Notification;
using GameServer.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameServer.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        // Persistence
        builder.ConfigurePersistence();
        
        // Identity
        builder.ConfigureIdentityServices();
        
        // Accounts
        builder.Services.AddScoped<IAccountService, AccountService>();
        
        // Notification
        builder.Services.AddScoped(typeof(INotificationDispatcher<>), typeof(NotificationDispatcher<>));
        
        // Utilities
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddHostedService<DataSeedHosted>();
    }
}
