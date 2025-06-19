using GameServer.Domain.Events.Accounts.Base;

namespace GameServer.Domain.Events.Accounts;

public class AccountDeactivatedEvent : AccountEvent
{
    public AccountDeactivatedEvent(Account account) : base(account)
    {
    }
}
