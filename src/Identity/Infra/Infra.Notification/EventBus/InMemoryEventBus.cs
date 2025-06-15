using Microsoft.Extensions.DependencyInjection;
using ServerGame.Application.Common.Interfaces.Notification.EventBus;

namespace Infra.Notification.EventBus;

public class InMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _provider;
    private readonly Dictionary<Type, List<Type>> _handlers = new();

    public InMemoryEventBus(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : class
        where THandler : IEventHandler<TEvent>
    {
        var evtType = typeof(TEvent);
        var handlerType = typeof(THandler);
        if (!_handlers.TryGetValue(evtType, out var list))
        {
            list = new List<Type>();
            _handlers[evtType] = list;
        }
        list.Add(handlerType);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    {
        var evtType = typeof(TEvent);
        if (!_handlers.TryGetValue(evtType, out var handlers))
            return;

        foreach (var handlerType in handlers)
        {
            using var scope = _provider.CreateScope();
            var handler = (IEventHandler<TEvent>)scope.ServiceProvider.GetRequiredService(handlerType);
            await handler.HandleAsync(@event, cancellationToken);
        }
    }
}
