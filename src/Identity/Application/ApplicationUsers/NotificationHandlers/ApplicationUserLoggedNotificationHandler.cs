using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.NotificationHandlers;
using ServerGame.Application.ApplicationUsers.Notifications;

namespace ServerGame.Application.ApplicationUsers.NotificationHandlers;

public class ApplicationUserLoggedNotificationHandler : INotificationHandler<ApplicationUserLoggedNotification>
{
    private readonly ILogger<AccountDeactivatedNotificationHandler> _logger;

    public ApplicationUserLoggedNotificationHandler(ILogger<AccountDeactivatedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ApplicationUserLoggedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ServerGame Application Event: {ApplicationUserEvent}", notification.GetType().Name);
        
        // TODO: Podemos adicionar um event bus aqui para enviar notificações ou realizar outras ações para outros microserviços.

        return Task.CompletedTask;
    }
}
