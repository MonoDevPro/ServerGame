using ServerGame.Domain.Events;

namespace ServerGame.Application.Common.Notifications;

public record DomainEventNotification<TDomainEvent>(TDomainEvent Event) 
    : INotification
    where TDomainEvent : IDomainEvent;
