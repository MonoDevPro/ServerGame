using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Adapters;
using ServerGame.Domain.Events.Accounts;

namespace ServerGame.Application.Accounts.NotificationHandlers;

public class AccountDeactivatedNotificationHandler : INotificationHandler<DomainEventNotification<AccountDomainDeactivatedEvent>>
{
    private readonly ILogger<AccountDeactivatedNotificationHandler> _logger;

    public AccountDeactivatedNotificationHandler(ILogger<AccountDeactivatedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<AccountDomainDeactivatedEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ServerGame Domain Event: {DomainEvent}", notification.GetType().Name);
        
        // TODO: Podemos adicionar um event bus aqui para enviar notificações ou realizar outras ações para outros microserviços.

        return Task.CompletedTask;
    }
}
