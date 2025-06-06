using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountLoggedOut : AccountEvent
{
    public AccountLoggedOut(Account account) : base(account)
    {
    }
}
