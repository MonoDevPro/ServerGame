using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Notifications.Adapters;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Application.Common.Interfaces.Notification.Dispatchers;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Events;

namespace Infra.Notification.Interceptors;

public class EventInterceptor(
    IEventDispatcher<IDomainEvent>? eventDispatcher, 
    INotificationDispatcher<INotification>? notificationDispatcher,
    ILogger<EventInterceptor> logger): SaveChangesInterceptor
{
    private readonly List<IDomainEvent> _pendingEvents = [];
    private readonly List<INotification> _pendingNotifications = [];
    
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        try
        {
            // Collect notifications from entities that implement IHasNotifications
            CollectDomainEventsWithNotifications(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EventInterceptor: Error during SavingChangesAsync");
            throw;
        }
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "UnitOfWorkInterceptor: Error during PostSaveChangesAsync");
            throw;
        }
        
        return eventData.EntitiesSavedCount;
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
