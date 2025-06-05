using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountActivatedEvent : AccountEvent
{
    public string Username { get; }

    public AccountActivatedEvent(
        long accountId,
        UsernameVO username
        ) : base(accountId)
    {
        Username = username.Value;
    }
}
