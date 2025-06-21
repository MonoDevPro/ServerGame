using GameServer.Application.Common.Interfaces.Notification.Dispatchers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameServer.Infrastructure.Notification;

public sealed class NotificationDispatcher<TNotification>(IMediator mediator, ILogger<NotificationDispatcher<TNotification>> logger) 
    : INotificationDispatcher<TNotification> where TNotification : INotification
{
    public async Task DispatchAsync(IEnumerable<TNotification> events, CancellationToken cancellationToken = default)
    {
        var exceptions = new List<Exception>();

        foreach (var @event in events)
        {
            try
            {
                logger.LogInformation("Dispatching notification: {EventType}", @event.GetType().Name);
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
