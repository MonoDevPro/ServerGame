using Game.Persistence;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Interfaces.Notification.Dispatchers;
using GameServer.Infrastructure.Identity;
using GameServer.Infrastructure.Notification;
using GameServer.Infrastructure.Services;
using GameServer.Infrastructure.Services.Accounts;
using GameServer.Infrastructure.Services.Characters;
using GameServer.Infrastructure.Services.Sessions;
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

        // Session management
        builder.Services.AddSingleton<SessionExpirationService>();
        builder.Services.AddHostedService<SessionExpirationService>(provider => 
            provider.GetRequiredService<SessionExpirationService>());

        // Accounts
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<ICurrentAccountService, CurrentAccountService>();

        // Characters
        builder.Services.AddScoped<ICharacterService, CharacterService>();
        builder.Services.AddScoped<ISelectedCharacterService, SelectedCharacterService>();

        // Notification
        builder.Services.AddScoped<INotificationDispatcher<INotification>, NotificationDispatcher<INotification>>();

        // Utilities
        builder.Services.AddSingleton(TimeProvider.System);
    }
}
