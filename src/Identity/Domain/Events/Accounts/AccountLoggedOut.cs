using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;

namespace ServerGame.Domain.Events.Accounts;

public class AccountLoggedOut : AccountEvent
{
    public AccountLoggedOut(Account account) : base(account)
    {
    }
}
