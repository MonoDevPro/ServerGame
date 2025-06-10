using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

public class AccountDomainLoginFailed : AccountDomainEvent
{
    public string IpAddress { get; }
    public DateTime LoginTime { get; }

    public AccountDomainLoginFailed(
        Account account,
        LoginInfo login
        ) : base(account)
    {
        IpAddress = login.LastLoginIp;
        LoginTime = login.LastLoginDate;
    }
}
