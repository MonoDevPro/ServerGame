using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountLoggedIn : AccountEvent
{
    public string Username { get; }
    public string IpAddress { get; }
    public DateTime LoginTime { get; }

    public AccountLoggedIn(
        long accountId,
        UsernameVO username,
        LoginInfoVO login
        ) : base(accountId)
    {
        Username = username.Value;
        IpAddress = login.LastLoginIp;
        LoginTime = login.LastLoginDate;
    }
}
