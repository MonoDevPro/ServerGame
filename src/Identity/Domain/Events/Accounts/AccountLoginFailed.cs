using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

public class AccountLoginFailed : AccountEvent
{
    public string IpAddress { get; }
    public DateTime LoginTime { get; }

    public AccountLoginFailed(
        Account account,
        LoginInfo login
        ) : base(account)
    {
        IpAddress = login.LastLoginIp;
        LoginTime = login.LastLoginDate;
    }
}
