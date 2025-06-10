using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;

namespace ServerGame.Domain.Events.Accounts;

public class AccountDomainActivatedEvent : AccountDomainEvent
{
    public AccountDomainActivatedEvent(
        Account account
        ) : base(account)
    {
    }
}
