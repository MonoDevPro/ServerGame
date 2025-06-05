using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountRoleRemovedEvent : AccountEvent
{
    public string Role { get; }

    public AccountRoleRemovedEvent(long accountId, RoleVO role) : base(accountId)
    {
        Role = role.Value;
    }
}
