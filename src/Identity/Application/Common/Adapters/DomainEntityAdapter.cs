using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Interfaces.Events;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Events;

namespace ServerGame.Application.Common.Adapters;

public class DomainEntityAdapter : IHasNotifications
{
    private readonly IHasDomainEvents _inner;
    private readonly List<INotification> _notifications = [];

    public DomainEntityAdapter(IHasDomainEvents inner)
        => _inner = inner;

    public IReadOnlyCollection<INotification> PendingNotifications
        => _notifications.AsReadOnly();

    // Chame este método no interceptor/pipeline antes do SaveChanges:
    public void CollectNotifications()
    {
        foreach (var domainEvent in _inner.Events)
        {
            // Mantém o tipo concreto no wrapper
            var notif = CreateNotification(domainEvent);
            _notifications.Add(notif);
        }
        _inner.ClearDomainEvents();
    }
    
    public void ClearPendingNotifications() 
        => _notifications.Clear();
    
    private static INotification CreateNotification(IDomainEvent domainEvent)
    {
        var concreteType = domainEvent.GetType();
        var wrapperType = typeof(DomainEventNotification<>).MakeGenericType(concreteType);
        return (INotification)Activator.CreateInstance(wrapperType, domainEvent)!;
    }
}
