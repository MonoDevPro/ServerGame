using GameServer.Domain.Events.Accounts.Base;
using GameServer.Domain.ValueObjects.Accounts;

namespace GameServer.Domain.Events.Accounts;

public class AccountLoginFailed : AccountEvent
{
    public string IpAddress { get; }
    public DateTimeOffset LoginTime { get; }

    public AccountLoginFailed(
        Account account,
        LoginInfo login
        ) : base(account)
    {
        IpAddress = login.LastLoginIp;
        LoginTime = login.LastLoginDate;
    }
}
