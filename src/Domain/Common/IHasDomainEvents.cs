namespace GameServer.Domain.Common;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> Events { get; }
    void ClearDomainEvents();
}
