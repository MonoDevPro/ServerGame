namespace ServerGame.Application.Common.Interfaces;

public interface IHasNotifications
{
    IReadOnlyCollection<INotification> PendingNotifications { get; }
    void ClearPendingNotifications();
}
