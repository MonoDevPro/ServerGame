using MediatR;

namespace ServerGame.Domain.Events;

public interface IDomainEvent : INotification
{
    /// <summary>
    /// Identificador único do evento.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Momento em que o evento ocorreu.
    /// </summary>
    DateTime OccurredOn { get; }
}
