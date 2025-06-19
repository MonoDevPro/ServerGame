using GameServer.Domain.Events.Accounts;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Handlers;

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
