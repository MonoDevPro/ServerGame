using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Application.Common.Interfaces.Notification.EventBus;
using ServerGame.Domain.Events;

namespace Infra.Notification.Dispatchers;

public class EventDispatcher<TEvent>(IEventBus eventBus, ILogger<EventDispatcher<TEvent>> logger) 
    : IEventDispatcher<TEvent> 
    where TEvent : IDomainEvent
{
    public virtual async Task DispatchAsync(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
    {
        var exceptions = new List<Exception>();

        foreach (var @event in events)
        {
            try
            {
                logger.LogDebug("Dispatching event: {EventType}", @event.GetType().Name);
                await eventBus.PublishAsync(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dispatch event: {EventType}", @event.GetType().Name);
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            throw new AggregateException("One or more events failed to dispatch", exceptions);
    }
}
