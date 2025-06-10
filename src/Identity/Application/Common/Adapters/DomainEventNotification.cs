using ServerGame.Domain.Events;

namespace ServerGame.Application.Common.Adapters;

public record DomainEventNotification<TDomainEvent>(TDomainEvent Event) : INotification
    where TDomainEvent : IDomainEvent;
