using GameServer.Domain.Events.Accounts.Base;

namespace GameServer.Domain.Events.Accounts;

public class AccountLoggedOut : AccountEvent
{
    public AccountLoggedOut(Account account) : base(account)
    {
    }
}
