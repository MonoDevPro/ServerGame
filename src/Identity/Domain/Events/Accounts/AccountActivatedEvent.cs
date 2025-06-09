using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;

namespace ServerGame.Domain.Events.Accounts;

public class AccountActivatedEvent : AccountEvent
{
    public AccountActivatedEvent(
        Account account
        ) : base(account)
    {
    }
}
