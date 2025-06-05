using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountRoleAddedEvent : AccountEvent
{
    public string Role { get; }

    public AccountRoleAddedEvent(long accountId, RoleVO role) : base(accountId)
    {
        Role = role.Value;
    }
}
