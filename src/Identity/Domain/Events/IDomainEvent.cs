namespace ServerGame.Domain.Events;

public interface IDomainEvent
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
