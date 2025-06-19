using MediatR;

namespace GameServer.Domain.Common;
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

/// <summary>
/// Classe base de todo evento de domínio: imutável, contém timestamp e, opcionalmente, um identificador único.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Identificador único do evento.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Momento em que o evento ocorreu.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
