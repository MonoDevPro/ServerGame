namespace ServerGame.Application.Common.Interfaces.Dispatchers;

public interface INotificationDispatcher<in TNotification> where TNotification : INotification
{
    Task DispatchAsync(IEnumerable<TNotification> notifications, CancellationToken cancellationToken = default);
}
