namespace ServerGame.Application.Common.Interfaces;

public interface IHasNotifications
{
    IReadOnlyCollection<INotification> Notifications { get; }
    void ClearEvents();
}
