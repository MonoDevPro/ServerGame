using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Adapters;
using ServerGame.Domain.Events.Accounts;

namespace ServerGame.Application.Accounts.NotificationHandlers;

public class AccountCreatedNotificationHandler(ILogger<AccountCreatedNotificationHandler> logger)
    : INotificationHandler<DomainEventNotification<AccountDomainCreatedEvent>>
{
    public Task Handle(DomainEventNotification<AccountDomainCreatedEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("ServerGame Domain Event: {DomainEvent}", notification.GetType().Name);
        
        // TODO: Podemos adicionar um event bus aqui para enviar notificações ou realizar outras ações para outros microserviços.

        return Task.CompletedTask;
    }
}
