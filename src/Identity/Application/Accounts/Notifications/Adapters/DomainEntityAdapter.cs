using ServerGame.Application.Common.Interfaces;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Events;

namespace ServerGame.Application.Accounts.Notifications.Adapters;

public class DomainEntityAdapter(IHasDomainEvents inner) : IHasNotifications
{
    private readonly List<INotification> _notifications = [];

    public IReadOnlyCollection<INotification> PendingNotifications
        => _notifications.AsReadOnly();

    // Chame este método no interceptor/pipeline antes do SaveChanges:
    public void CollectNotifications()
    {
        foreach (var domainEvent in inner.Events)
        {
            // Mantém o tipo concreto no wrapper
            var notif = CreateNotification(domainEvent);
            _notifications.Add(notif);
        }
        inner.ClearDomainEvents();
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
