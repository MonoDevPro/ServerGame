namespace ServerGame.Application.Common.Interfaces.Events;

public interface INotificationDispatcher
{
    Task DispatchAsync(IEnumerable<INotification> events, CancellationToken cancellationToken = default);
}
