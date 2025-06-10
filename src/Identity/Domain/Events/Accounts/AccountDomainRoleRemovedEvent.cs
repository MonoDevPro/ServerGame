using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

public class AccountDomainRoleRemovedEvent : AccountDomainEvent
{
    public string Role { get; }

    public AccountDomainRoleRemovedEvent(Account account, Role role) : base(account)
    {
        Role = role;
    }
}
