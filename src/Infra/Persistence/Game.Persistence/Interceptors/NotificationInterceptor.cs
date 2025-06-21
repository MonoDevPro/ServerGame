using GameServer.Application.Common.Interfaces.Notification.Dispatchers;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using GameServer.Domain.Common;
using MediatR;

namespace Game.Persistence.Interceptors;

public class NotificationInterceptor(INotificationDispatcher<INotification> notificationDispatcher)
    : SaveChangesInterceptor
{
    private readonly List<INotification> _pendingNotifications = [];
    
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        CollectNotifications(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, 
        InterceptionResult<int> result)
    {
        // para cobertura de código sync, se você usar SaveChanges()
        CollectNotifications(eventData.Context);
        return base.SavingChanges(eventData, result);
    }
    
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await DispatchPendingAsync(cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        // para cobertura de código sync, se você usar SaveChanges()
        DispatchPendingAsync(CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    
    private void CollectNotifications(DbContext? context)
    {
        if (context == null) return;

        var entities = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.Events.Count > 0)
            .ToList();

        foreach (var entity in entities)
        {
            _pendingNotifications.AddRange(entity.Events);
            entity.ClearDomainEvents();
        }
    }
    
    private async Task DispatchPendingAsync(CancellationToken cancellationToken)
    {
        if (_pendingNotifications.Count == 0) return;
        try
        {
            await notificationDispatcher.DispatchAsync(_pendingNotifications, cancellationToken);
        }
        finally
        {
            _pendingNotifications.Clear();
        }
    }
}
