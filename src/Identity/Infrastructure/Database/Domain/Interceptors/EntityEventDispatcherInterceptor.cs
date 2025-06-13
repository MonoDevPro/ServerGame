using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ServerGame.Application.Accounts.Adapters;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Events;
using ServerGame.Infrastructure.Database.Common.Interceptors.Interfaces;

namespace ServerGame.Infrastructure.Database.Domain.Interceptors;

public class EntityEventDispatcherInterceptor<TContext>(
    IEventDispatcher<IDomainEvent>? eventDispatcher, 
    INotificationDispatcher<INotification>? notificationDispatcher
    ): IPostSaveInterceptor<TContext>, IPreSaveInterceptor<TContext>
    where TContext : DbContext
    
{
    private readonly List<IDomainEvent> _pendingEvents = [];
    private readonly List<INotification> _pendingNotifications = [];
    
    public Task PreSaveChangesAsync(DbContextEventData contextData, CancellationToken cancellationToken = default)
    {
        // Collect notifications from entities that implement IHasNotifications
        CollectDomainEventsWithNotifications(contextData.Context);
        return Task.CompletedTask;
    }
    
    public async Task PostSaveChangesAsync(SaveChangesCompletedEventData contextData, CancellationToken cancellationToken = default)
    {
        if (eventDispatcher is not null)
        {
            if (_pendingEvents.Count > 0)
            {
                await eventDispatcher.DispatchAsync(_pendingEvents, cancellationToken);
                _pendingEvents.Clear(); // Clear after dispatching
            }
        }
        
        if (notificationDispatcher is not null)
        {
            if (_pendingNotifications.Count > 0)
            {
                await notificationDispatcher.DispatchAsync(_pendingNotifications, cancellationToken);
                _pendingNotifications.Clear(); // Clear after dispatching
            }
        }
    }
    
    private void CollectDomainEventsWithNotifications(DbContext? context)
    {
        if (context == null) return;
        
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
