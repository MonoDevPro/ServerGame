using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ServerGame.Application.Common.Adapters;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Events;
using ServerGame.Infrastructure.Database.Common.Interceptors;

namespace ServerGame.Infrastructure.Database.Domain.Interceptors;

public class EntityEventDispatcherInterceptor(
    IEventDispatcher<IDomainEvent>? eventDispatcher, INotificationDispatcher<INotification>? notificationDispatcher
    ) : SaveInterceptor
{
    private readonly List<IDomainEvent> _pendingEvents = [];
    private readonly List<INotification> _pendingNotifications = [];
    
    public override Task PreSaveChangesAsync(DbContext context)
    {
        // Collect notifications from entities that implement IHasNotifications
        CollectDomainEventsWithNotifications(context);
        return Task.CompletedTask;
    }
    
    public override async Task PostSaveChangesAsync(DbContext context)
    {
        if (eventDispatcher is not null)
        {
            if (_pendingEvents.Count > 0)
            {
                await eventDispatcher.DispatchAsync(_pendingEvents);
                _pendingEvents.Clear(); // Clear after dispatching
            }
        }
        
        if (notificationDispatcher is not null)
        {
            if (_pendingNotifications.Count > 0)
            {
                await notificationDispatcher.DispatchAsync(_pendingNotifications);
                _pendingNotifications.Clear(); // Clear after dispatching
            }
        }
    }
    
    private void CollectDomainEventsWithNotifications(DbContext context)
    {
        // Collect domain events from entities that implement IHasDomainEvents
        var entitiesWithDomainEvents = GetEntitiesWithDomainEvents(context);

        foreach (var entity in entitiesWithDomainEvents)
        {
            if (entity.Events.Count > 0)
            {
                _pendingEvents.AddRange(entity.Events);
                
                var adapter = new DomainEntityAdapter(entity);
                adapter.CollectNotifications();
                _pendingNotifications.AddRange(adapter.PendingNotifications);
                // Clear the domain events after collecting
                entity.ClearDomainEvents();
            }
        }
    }
    
    private IEnumerable<IHasDomainEvents> GetEntitiesWithDomainEvents(DbContext context)
    {
        return context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity);
    }
}
