using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;

namespace ServerGame.Domain.Events.Accounts;

public class AccountDomainDeactivatedEvent : AccountDomainEvent
{
    public AccountDomainDeactivatedEvent(Account account) : base(account)
    {
    }
}
