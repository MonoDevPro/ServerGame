using System.ComponentModel.DataAnnotations.Schema;

namespace ServerGame.Domain.Entities;

public abstract class BaseEntity : IHasDomainEvents
{
    public long Id { get; private set; } // identidade
    
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
        return Id.Equals(other.Id);
    }

    public static bool operator ==(BaseEntity? a, BaseEntity? b)
    {
        if (ReferenceEquals(a, b)) return true;
        return a != null && a.Equals(b);
    }

    public static bool operator !=(BaseEntity? a, BaseEntity? b) => !(a == b);

    public override int GetHashCode()
        => (GetUnproxiedType(this).ToString() + Id)
            .GetHashCode(); // hash baseado em Id

    internal static Type GetUnproxiedType(object obj)
    {
        const string efCoreProxyPrefix = "Castle.Proxies.";
        const string nHibernateProxyPostfix = "Proxy";
        var type = obj.GetType();
        var name = type.ToString();
        if (name.Contains(efCoreProxyPrefix) || name.EndsWith(nHibernateProxyPostfix))
            return type.BaseType!;
        return type;
    }
}
