using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Adapters;
using ServerGame.Domain.Events.Accounts;

namespace ServerGame.Application.Accounts.NotificationHandlers;

public class AccountCreatedNotificationHandler : INotificationHandler<DomainEventNotification<AccountDomainCreatedEvent>>
{
    private readonly ILogger<AccountCreatedNotificationHandler> _logger;

    public AccountCreatedNotificationHandler(ILogger<AccountCreatedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<AccountDomainCreatedEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ServerGame Domain Event: {DomainEvent}", notification.GetType().Name);
        
        // TODO: Podemos adicionar um event bus aqui para enviar notificações ou realizar outras ações para outros microserviços.

        return Task.CompletedTask;
    }
}
