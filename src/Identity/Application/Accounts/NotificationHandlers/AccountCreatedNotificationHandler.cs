using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Notifications;

namespace ServerGame.Application.Accounts.NotificationHandlers;

public class AccountCreatedNotificationHandler : INotificationHandler<AccountCreatedNotification>
{
    private readonly ILogger<AccountCreatedNotificationHandler> _logger;

    public AccountCreatedNotificationHandler(ILogger<AccountCreatedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AccountCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ServerGame Domain Event: {DomainEvent}", notification.GetType().Name);
        
        // TODO: Podemos adicionar um event bus aqui para enviar notificações ou realizar outras ações para outros microserviços.

        return Task.CompletedTask;
    }
}
