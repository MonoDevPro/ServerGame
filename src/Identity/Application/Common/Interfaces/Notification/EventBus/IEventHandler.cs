namespace ServerGame.Application.Common.Interfaces.Notification.EventBus;

// Handler de eventos
public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
