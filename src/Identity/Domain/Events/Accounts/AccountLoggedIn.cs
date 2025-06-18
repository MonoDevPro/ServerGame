using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

public class AccountLoggedIn : AccountEvent
{
    public LoginInfo LoginInfo { get; }

    public AccountLoggedIn(
        Account account,
        LoginInfo loginInfo) : base(account)
    {
        LoginInfo = loginInfo;
    }
}
