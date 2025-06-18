using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ServerGame.Application.Common.Interfaces.Notification.Dispatchers;
using ServerGame.Domain.Entities;

namespace Infra.Notification.Interceptors;

public class NotificationInterceptor(INotificationDispatcher<INotification>? notificationDispatcher)
    : SaveChangesInterceptor
{
    private readonly List<INotification> _pendingNotifications = [];
    
    public Task PreSaveChangesAsync(DbContextEventData contextData, CancellationToken cancellationToken = default)
    {
        // Collect notifications from entities that implement IHasNotifications
        CollectNotifications(contextData.Context);
        return Task.CompletedTask;
    }
    
    public async Task PostSaveChangesAsync(SaveChangesCompletedEventData contextData, CancellationToken cancellationToken = default)
    {
        if (notificationDispatcher is not null)
        {
            if (_pendingNotifications.Count > 0)
            {
                await notificationDispatcher.DispatchAsync(_pendingNotifications, cancellationToken);
                _pendingNotifications.Clear(); // Clear after dispatching
            }
        }
    }
    
    private void CollectNotifications(DbContext? context)
    {
        if (context == null) return;
        
        // Collect domain events from entities that implement IHasDomainEvents
        var entitiesWithDomainEvents = GetEntitiesWithNotifications(context);

        foreach (var entity in entitiesWithDomainEvents)
        {
            if (entity.Events.Count > 0)
            {
                _pendingNotifications.AddRange(entity.Events);
                
                // Clear the notifications after collecting
                entity.ClearDomainEvents();
            }
        }
    }
    
    private static IEnumerable<IHasDomainEvents> GetEntitiesWithNotifications(DbContext context)
    {
        return context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity);
    }
}
