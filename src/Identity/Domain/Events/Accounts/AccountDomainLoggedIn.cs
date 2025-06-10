using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

public class AccountDomainLoggedIn : AccountDomainEvent
{
    public LoginInfo LoginInfo { get; }

    public AccountDomainLoggedIn(
        Account account,
        LoginInfo loginInfo) : base(account)
    {
        LoginInfo = loginInfo;
    }
}
