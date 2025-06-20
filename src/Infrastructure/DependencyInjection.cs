using Game.Persistence;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces.Notification.Dispatchers;
using GameServer.Infrastructure.Identity;
using GameServer.Infrastructure.Notification;
using GameServer.Infrastructure.Services;
using MediatR;
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
        builder.Services.AddScoped<INotificationDispatcher<INotification>, NotificationDispatcher<INotification>>();
        
        // Utilities
        builder.Services.AddSingleton(TimeProvider.System);
    }
}
