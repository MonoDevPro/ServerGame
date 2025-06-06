using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountRoleAddedEvent : AccountEvent
{
    public Role Role { get; }

    public AccountRoleAddedEvent(Account account, Role role) : base(account)
    {
        Role = role;
    }
}
