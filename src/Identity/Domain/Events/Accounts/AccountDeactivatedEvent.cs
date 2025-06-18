using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;

namespace ServerGame.Domain.Events.Accounts;

public class AccountDeactivatedEvent : AccountEvent
{
    public AccountDeactivatedEvent(Account account) : base(account)
    {
    }
}
