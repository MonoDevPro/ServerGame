using System.ComponentModel.DataAnnotations.Schema;
using GameServer.Shared.Domain.Events;

namespace GameServer.Shared.Domain.Entities;

public abstract class BaseEntity
{
    public long Id { get; private set; } // identidade
    
    private readonly List<DomainEvent> _domainEvents = []; // eventos de domínio :contentReference[oaicite:9]{index=9}
    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;

    protected BaseEntity() { }

    protected BaseEntity(long id)
    {
        Id = id;
    }

    public void AddDomainEvent(DomainEvent eventItem)
        => _domainEvents.Add(eventItem);

    public void RemoveDomainEvent(DomainEvent eventItem)
        => _domainEvents.Remove(eventItem);

    public IReadOnlyCollection<DomainEvent> GetDomainEvents()
        => _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetUnproxiedType(this) != GetUnproxiedType(other)) return false;
        //if (Id is null || other.Id is null) return false;
        return Id.Equals(other.Id);
    }

    public static bool operator ==(BaseEntity? a, BaseEntity? b)
    {
        if (ReferenceEquals(a, b)) return true;
        //if (a.Id is null || b.Id is null) return false;
        return a != null && a.Equals(b);
    }

    public static bool operator !=(BaseEntity? a, BaseEntity? b) => !(a == b);

    public override int GetHashCode()
        => (GetUnproxiedType(this).ToString() + Id)
            .GetHashCode(); // hash baseado em Id :contentReference[oaicite:10]{index=10}

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
