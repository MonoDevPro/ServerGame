using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

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
