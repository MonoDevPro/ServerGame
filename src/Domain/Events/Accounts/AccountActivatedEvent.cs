using GameServer.Domain.Events.Accounts.Base;

namespace GameServer.Domain.Events.Accounts;

public class AccountActivatedEvent : AccountEvent
{
    public AccountActivatedEvent(
        Account account
        ) : base(account)
    {
    }
}
