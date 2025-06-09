using ServerGame.Domain.Events;

namespace ServerGame.Application.Accounts.Notifications.Base;

public abstract record AccountNotification(Guid EventId, DateTime OccurredOn) : INotification, IDomainEvent
{
    protected AccountNotification() : this(Guid.NewGuid(), DateTime.UtcNow) { }
}
