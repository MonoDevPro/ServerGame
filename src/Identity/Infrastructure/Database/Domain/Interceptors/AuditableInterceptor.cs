using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Domain.Entities;
using ServerGame.Infrastructure.Database.Common.Interceptors.Interfaces;

namespace ServerGame.Infrastructure.Database.Domain.Interceptors;

public class AuditableInterceptor<TContext>(
    IUser user,
    TimeProvider dateTime) : IPreSaveInterceptor<TContext>
    where TContext : DbContext
{
    public Task PreSaveChangesAsync(DbContextEventData contextData, CancellationToken cancellationToken = default)
    {
        UpdateEntities(contextData.Context);
        return Task.CompletedTask;
    }
    
    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = dateTime.GetUtcNow();
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = user.Id;
                    entry.Entity.Created = utcNow;
                } 
                entry.Entity.LastModifiedBy = user.Id;
                entry.Entity.LastModified = utcNow;
            }
        }
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r => 
            r.TargetEntry != null && 
            r.TargetEntry.Metadata.IsOwned() && 
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
