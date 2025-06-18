using Microsoft.Extensions.Logging;
using ServerGame.Domain.Events.Accounts;

namespace ServerGame.Application.Accounts.Notifications;

public class AccountDeactivatedNotificationHandler(ILogger<AccountDeactivatedNotificationHandler> logger)
    : INotificationHandler<AccountDeactivatedEvent>
{
    public Task Handle(AccountDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing ApplicationUserDeactivatedEvent for UserId {UserId}",
            notification.AccountId);
        
        // TODO: Podemos adicionar um event bus aqui para enviar notificações ou realizar outras ações para outros microserviços.

        return Task.CompletedTask;
    }
}
