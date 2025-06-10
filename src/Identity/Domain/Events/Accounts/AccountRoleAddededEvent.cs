using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

public class AccountDomainRoleAddedEvent : AccountDomainEvent
{
    public Role Role { get; }

    public AccountDomainRoleAddedEvent(Account account, Role role) : base(account)
    {
        Role = role;
    }
}
