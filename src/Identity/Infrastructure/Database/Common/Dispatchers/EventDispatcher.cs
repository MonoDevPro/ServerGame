using MediatR;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Domain.Events;

namespace ServerGame.Infrastructure.Database.Common.Dispatchers;

public class EventDispatcher(
    ILogger<EventDispatcher> logger
    ) : IEventDispatcher<IDomainEvent>
{
    public virtual Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        var exceptions = new List<Exception>();

        foreach (var @event in events)
        {
            try
            {
                logger.LogDebug("Dispatching event: {EventType}", @event.GetType().Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dispatch event: {EventType}", @event.GetType().Name);
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            throw new AggregateException("One or more events failed to dispatch", exceptions);
        
        return Task.CompletedTask;
    }
}
