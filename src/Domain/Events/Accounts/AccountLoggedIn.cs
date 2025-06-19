using GameServer.Domain.Events.Accounts.Base;
using GameServer.Domain.ValueObjects.Accounts;

namespace GameServer.Domain.Events.Accounts;

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
