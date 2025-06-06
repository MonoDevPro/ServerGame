using Microsoft.Extensions.Logging;
using ServerGame.Domain.Events;

namespace ServerGame.Application.Accounts.EventHandlers;

public class AccountDeactivatedEventHandler : INotificationHandler<AccountDeactivatedEvent>
{
    private readonly ILogger<AccountDeactivatedEventHandler> _logger;

    public AccountDeactivatedEventHandler(ILogger<AccountDeactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AccountDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ServerGame Domain Event: {DomainEvent}", notification.GetType().Name);
        
        // TODO: Podemos adicionar um event bus aqui para enviar notificações ou realizar outras ações para outros microserviços.

        return Task.CompletedTask;
    }
}
