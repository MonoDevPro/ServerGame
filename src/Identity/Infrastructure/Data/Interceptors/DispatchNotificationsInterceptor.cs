using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Adapters;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Interfaces.Events;
using ServerGame.Domain.Entities;

namespace ServerGame.Infrastructure.Data.Interceptors;

public class DispatchNotificationsInterceptor : SaveChangesInterceptor
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<DispatchNotificationsInterceptor> _logger;
    private readonly List<INotification> _pendingEvents = [];

    public DispatchNotificationsInterceptor(
        INotificationDispatcher dispatcher,
        ILogger<DispatchNotificationsInterceptor> logger
    )
    {
        _dispatcher = dispatcher;
        _logger     = logger;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            var adapters = new List<IHasNotifications>();

            // 1) Adapters nos domain-entities
            foreach (var entry in eventData.Context.ChangeTracker.Entries<IHasDomainEvents>())
            {
                var adapter = new DomainEntityAdapter(entry.Entity);
                adapter.CollectNotifications();
                adapters.Add(adapter);
            }

            // 2) Entidades que já expõem notificações
            adapters.AddRange(
                eventData.Context.ChangeTracker
                    .Entries<IHasNotifications>()
                    .Select(e => e.Entity)
            );

            _pendingEvents.AddRange(adapters.SelectMany(a => a.PendingNotifications));

            // 3) Limpeza
            foreach (var a in adapters)
                a.ClearPendingNotifications();


            _logger.LogDebug("DispatchEventsInterceptor: Collected {Count} events", _pendingEvents.Count);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_pendingEvents.Count > 0)
        {
            _logger.LogInformation("DispatchEventsInterceptor: Dispatching {Count} events", _pendingEvents.Count);
            try
            {
                await _dispatcher.DispatchAsync(_pendingEvents, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DispatchEventsInterceptor: Error during DispatchAsync");
                throw;
            }
            finally
            {
                _pendingEvents.Clear();
            }
        }
        else
        {
            _logger.LogDebug("DispatchEventsInterceptor: No events to dispatch");
        }

        return result;
    }
}
