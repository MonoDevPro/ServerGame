using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Notifications;
using ServerGame.Domain.Entities;

namespace ServerGame.Infrastructure.Data.Interceptors;

public class DispatchEventsInterceptor(
    IMediator mediator, 
    ILogger<DispatchEventsInterceptor> logger
    ) : SaveChangesInterceptor
{
    // Usar AsyncLocal para isolar eventos por operação de salvamento
    private static readonly AsyncLocal<List<INotification>> _pendingEvents = new();

    private List<INotification> PendingEvents =>
        _pendingEvents.Value ??= new List<INotification>();

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        
        try
        {
            // Coleta os eventos de domínio e identidade
            var domainEvents = GetDomainEvents(eventData);
            var identityEvents = GetIdentityEvents(eventData);
            
            // Adiciona todos os eventos à lista local
            PendingEvents.AddRange(domainEvents);
            PendingEvents.AddRange(identityEvents);

            logger.LogDebug("DispatchEventsInterceptor: Collected {Count} events for dispatch", 
                PendingEvents.Count);

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DispatchEventsInterceptor: Error during SavingChangesAsync");
            // Limpa eventos em caso de erro
            PendingEvents.Clear();
            throw;
        }
    }
    
    private List<INotification> GetDomainEvents(DbContextEventData eventData)
    {
        var entries = eventData.Context?.ChangeTracker
            .Entries()
            .Where(e => e is
            {
                Entity: IHasDomainEvents
            })
            .Select(e => (IHasDomainEvents)e.Entity)
            .ToList();

        if (entries is null || entries.Count == 0)
            return new List<INotification>();
        
        var domainEvents = new List<INotification>();

        foreach (var entry in entries)
        {
            // Para cada domain event, cria uma notification genérica
            foreach (var domainEvent in entry.Events)
            {
                try
                {
                    // Cria em tempo de execução o tipo DomainEventNotification<TEvent>
                    var notificationType = typeof(DomainEventNotification<>)
                        .MakeGenericType(domainEvent.GetType());
            
                    // Instancia: new DomainEventNotification<ConcreteEvent>(domainEvent)
                    var notification = (INotification)Activator
                        .CreateInstance(notificationType, domainEvent)!;
            
                    domainEvents.Add(notification);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "DispatchEventsInterceptor: Error creating notification for domain event {EventType}", 
                        domainEvent.GetType().Name);
                    throw;
                }
            }
        }
        
        // Limpa os eventos na entidade para não duplicar
        foreach (var entry in entries)
            entry.ClearEvents();
        
        return domainEvents;
    }
    
    private List<INotification> GetIdentityEvents(DbContextEventData eventData)
    {
        var entries = eventData.Context?.ChangeTracker
            .Entries()
            .Where(e => e is
            {
                Entity: IHasNotifications
            })
            .Select(e => (IHasNotifications)e.Entity)
            .ToList();

        if (entries is null || entries.Count == 0)
            return new List<INotification>();
        
        var identityEvents = entries
            .SelectMany(e => e.Notifications)
            .ToList();
        
        // Padroniza o método de limpeza - assumindo que ApplicationUser também implementa ClearDomainEvents
        // Se não implementar, você pode criar um método específico ou interface
        foreach (var entry in entries)
        {
            // Opção 1: Se ApplicationUser implementa IHasDomainEvents
            entry.ClearEvents();
        }

        return identityEvents;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("DispatchEventsInterceptor: SavedChangesAsync called with result: {Result}", result);
        
        try
        {
            await DispatchPendingEventsAsync(cancellationToken);
            return result; // Retorna o resultado original, não chama base novamente
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DispatchEventsInterceptor: Error during event dispatch");
            throw;
        }
        finally
        {
            // Sempre limpa os eventos pendentes
            PendingEvents.Clear();
        }
    }

    private async Task DispatchPendingEventsAsync(CancellationToken cancellationToken)
    {
        
        if (PendingEvents.Count == 0)
        {
            logger.LogDebug("DispatchEventsInterceptor: No events to dispatch");
            return;
        }
        
        logger.LogInformation("DispatchEventsInterceptor: Dispatching {Count} events", PendingEvents.Count);
        
        var exceptions = new List<Exception>();

        foreach (var @event in PendingEvents)
        {
            try
            {
                logger.LogDebug("DispatchEventsInterceptor: Dispatching event {EventType}", @event.GetType().Name);
                await mediator.Publish(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DispatchEventsInterceptor: Error dispatching event {EventType}", @event.GetType().Name);
                exceptions.Add(ex);
                
                // Você pode decidir a estratégia aqui:
                // - Continuar com outros eventos (atual)
                // - Parar na primeira exceção (throw)
                // - Coletar todas as exceções e lançar AggregateException
            }
        }

        // Opcional: Se quiser lançar todas as exceções coletadas
        if (exceptions.Count > 0)
        {
            logger.LogWarning("DispatchEventsInterceptor: {Count} events failed to dispatch", exceptions.Count);
            // Descomente se quiser falhar completamente em caso de erro
            // throw new AggregateException("One or more events failed to dispatch", exceptions);
        }
        
        logger.LogInformation("DispatchEventsInterceptor: Finished dispatching events");
    }
}
