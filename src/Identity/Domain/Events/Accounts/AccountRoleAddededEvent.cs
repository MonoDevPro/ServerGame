using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

public class AccountRoleAddedEvent : AccountEvent
{
    public Role Role { get; }

    public AccountRoleAddedEvent(Account account, Role role) : base(account)
    {
        Role = role;
    }
}
