using Microsoft.Extensions.Logging;
using ServerGame.Domain.Events.Accounts;

namespace ServerGame.Application.Accounts.Notifications;

public class AccountCreatedNotificationHandler(ILogger<AccountCreatedNotificationHandler> logger)
    : INotificationHandler<AccountCreatedEvent>
{
    public Task Handle(AccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("ServerGame Domain Event: {DomainEvent}", notification.GetType().Name);
        
        // TODO: Podemos adicionar um event bus aqui para enviar notificações ou realizar outras ações para outros microserviços.

        return Task.CompletedTask;
    }
}
