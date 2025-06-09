namespace ServerGame.Domain.Entities;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> Events { get; }
    void ClearEvents();
}
