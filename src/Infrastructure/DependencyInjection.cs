using Game.Persistence;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Accounts.Services.Current;
using GameServer.Application.Characters.Services;
using GameServer.Application.Characters.Services.Current;
using GameServer.Application.Common.Interfaces.Notification.Dispatchers;
using GameServer.Application.Session;
using GameServer.Infrastructure.Notification;
using GameServer.Infrastructure.Services;
using GameServer.Infrastructure.Services.Accounts;
using GameServer.Infrastructure.Services.Accounts.Current;
using GameServer.Infrastructure.Services.Characters;
using GameServer.Infrastructure.Services.Characters.Current;
using GameServer.Infrastructure.Services.Sessions;
using GameServer.Infrastructure.Services.Users.Identity;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameServer.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        // Persistence & Identity
        builder.ConfigurePersistence();
        builder.ConfigureIdentityServices();

        // Utilities
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton(TimeProvider.System);

        // Métricas
        builder.Services.AddMetrics();

        // Session management
        builder.Services.AddScoped<ISessionManager, SessionManagerService>();
        builder.Services.AddScoped<ISessionDataStore, SessionDataStore>();

        // Accounts
        builder.Services.AddScoped<IAccountCommandService, AccountService>();
        builder.Services.AddScoped<IAccountQueryService, AccountService>();
        builder.Services.AddScoped<ICurrentAccountService, CurrentAccountService>();

        // Characters
        builder.Services.AddScoped<ICharacterCommandService, CharacterService>();
        builder.Services.AddScoped<ICharacterQueryService, CharacterService>();
        builder.Services.AddScoped<ICurrentCharacterList, CurrentCharacterList>();
        builder.Services.AddScoped<ICurrentCharacterSelection, CurrentCharacterSelection>();
        builder.Services.AddScoped<ICurrentCharacterSelector, CurrentCharacterSelector>();

        // Notification
        builder.Services.AddScoped<INotificationDispatcher<INotification>, NotificationDispatcher<INotification>>();
    }
}
