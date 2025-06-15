namespace ServerGame.Application.Common.Interfaces.Notification.EventBus;

public interface IEventBus
{
    // Publica um evento (pode ser DomainEvent ou IntegrationEvent)
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
    
    // Registra handler para eventos
    void Subscribe<TEvent, THandler>()
        where TEvent : class
        where THandler : IEventHandler<TEvent>;
}
