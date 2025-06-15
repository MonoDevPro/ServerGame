using MediatR;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Application.Common.Interfaces.Notification.Dispatchers;

namespace Infra.Notification.Dispatchers;

public class NotificationDispatcher<TNotification>(IMediator mediator, ILogger<NotificationDispatcher<TNotification>> logger) 
    : INotificationDispatcher<TNotification> where TNotification : INotification
{
    public virtual async Task DispatchAsync(IEnumerable<TNotification> events, CancellationToken cancellationToken = default)
    {
        var exceptions = new List<Exception>();

        foreach (var @event in events)
        {
            try
            {
                logger.LogDebug("Dispatching notification: {EventType}", @event.GetType().Name);
                await mediator.Publish(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dispatch notification: {EventType}", @event.GetType().Name);
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException("One or more notification failed to dispatch", exceptions);
        }
    }
}
