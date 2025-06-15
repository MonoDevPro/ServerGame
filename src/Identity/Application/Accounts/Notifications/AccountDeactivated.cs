using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Notifications.Adapters;
using ServerGame.Domain.Events.Accounts;

namespace ServerGame.Application.Accounts.Notifications;

public class AccountDeactivatedNotificationHandler(ILogger<AccountDeactivatedNotificationHandler> logger)
    : INotificationHandler<DomainEventNotification<AccountDomainDeactivatedEvent>>
{
    public Task Handle(DomainEventNotification<AccountDomainDeactivatedEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing ApplicationUserDeactivatedEvent for UserId {UserId} with Username {UserName} and Email {Email}",
            notification.Event.AccountId, 
            notification.Event.Username, 
            notification.Event.Email);
        
        // TODO: Podemos adicionar um event bus aqui para enviar notificações ou realizar outras ações para outros microserviços.

        return Task.CompletedTask;
    }
}
