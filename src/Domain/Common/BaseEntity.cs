using System.ComponentModel.DataAnnotations.Schema;

namespace GameServer.Domain.Common;

public abstract class BaseEntity : IHasDomainEvents
{
    public long Id { get; private set; } // identidade
    
    public bool IsActive { get; protected set; } // Soft delete flag
    
    private readonly List<IDomainEvent> _domainEvents = []; // eventos de domínio :contentReference[oaicite:9]{index=9}
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> Events => _domainEvents;

    // Construtor sem parâmetros é necessário para o EF Core
    protected BaseEntity() { }

    protected BaseEntity(long id)
    {
        Id = id;
    }

    public void AddDomainEvent(IDomainEvent domainEventItem)
        => _domainEvents.Add(domainEventItem);

    public void RemoveDomainEvent(IDomainEvent domainEventItem)
        => _domainEvents.Remove(domainEventItem);

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
        => _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetUnproxiedType(this) != GetUnproxiedType(other)) return false;

        // Somente Id: se ainda não foi atribuído (0), considere diferente
        if (Id == default || other.Id == default) 
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
        => (GetUnproxiedType(this).ToString() + Id)
            .GetHashCode(); // hash baseado em Id

    internal static Type GetUnproxiedType(object obj)
    {
        var type = obj.GetType();
        var name = type.ToString();
        if (name.StartsWith("Castle.Proxies.") && type.BaseType is not null)
            return type.BaseType;
        return type;
    }
}
