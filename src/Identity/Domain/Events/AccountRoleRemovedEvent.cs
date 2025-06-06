using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountRoleRemovedEvent : AccountEvent
{
    public string Role { get; }

    public AccountRoleRemovedEvent(Account account, Role role) : base(account)
    {
        Role = role;
    }
}
