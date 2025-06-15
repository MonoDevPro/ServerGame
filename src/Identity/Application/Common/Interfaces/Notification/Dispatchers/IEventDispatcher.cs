using ServerGame.Domain.Events;

namespace ServerGame.Application.Common.Interfaces.Dispatchers;

public interface IEventDispatcher<in TEvent> where TEvent : IDomainEvent
{
    Task DispatchAsync(IEnumerable<TEvent> events, CancellationToken cancellationToken = default);
}
