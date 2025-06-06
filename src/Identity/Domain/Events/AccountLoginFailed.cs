using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

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
