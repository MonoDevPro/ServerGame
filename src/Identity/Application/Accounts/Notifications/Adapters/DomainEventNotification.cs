using ServerGame.Domain.Events;

namespace ServerGame.Application.Accounts.Notifications.Adapters;

public record DomainEventNotification<TDomainEvent>(TDomainEvent Event) : INotification
    where TDomainEvent : IDomainEvent
{
}
