using MediatR;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Domain.Events;
using ServerGame.Infrastructure.Data.Events;

namespace ServerGame.Infrastructure.Database.Domain.Dispatchers;

public class EventDispatcher(IMediator mediator, ILogger<NotificationDispatcher> logger) : IEventDispatcher<IDomainEvent>
{
    public virtual async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        var exceptions = new List<Exception>();

        foreach (var @event in events)
        {
            try
            {
                logger.LogDebug("Dispatching event: {EventType}", @event.GetType().Name);
                await mediator.Publish(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dispatch event: {EventType}", @event.GetType().Name);
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException("One or more events failed to dispatch", exceptions);
        }
    }
}
