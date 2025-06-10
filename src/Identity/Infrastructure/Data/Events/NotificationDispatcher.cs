using MediatR;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Events;

namespace ServerGame.Infrastructure.Data.Events;

public class NotificationDispatcher(IMediator mediator, ILogger<NotificationDispatcher> logger) : INotificationDispatcher
{
    public virtual async Task DispatchAsync(IEnumerable<INotification> events, CancellationToken cancellationToken = default)
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
