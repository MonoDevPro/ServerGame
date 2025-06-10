using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;

namespace ServerGame.Domain.Events.Accounts;

public class AccountDomainLoggedOut : AccountDomainEvent
{
    public AccountDomainLoggedOut(Account account) : base(account)
    {
    }
}
