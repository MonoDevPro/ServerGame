using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Events;
using ServerGame.Infrastructure.Database.Common.Interceptors.Interfaces;

namespace ServerGame.Infrastructure.Database.Application.Interceptors;

public class ApplicationNotificationInterceptor<TContext>(
    INotificationDispatcher<INotification>? notificationDispatcher
    ) : IPostSaveInterceptor<TContext>, IPreSaveInterceptor<TContext>
    where TContext : DbContext
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
            if (entity.PendingNotifications.Count > 0)
            {
                _pendingNotifications.AddRange(entity.PendingNotifications);
                
                // Clear the notifications after collecting
                entity.ClearPendingNotifications();
            }
        }
    }
    
    private static IEnumerable<IHasNotifications> GetEntitiesWithNotifications(DbContext context)
    {
        return context.ChangeTracker
            .Entries<IHasNotifications>()
            .Select(e => e.Entity);
    }
}
