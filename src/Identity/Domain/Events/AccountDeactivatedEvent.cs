using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountDeactivatedEvent : AccountEvent
{
    public string Username { get; }

    public AccountDeactivatedEvent(
        Account account
        ) : base(account)
    {
        Username = account.Username.Value;
    }
}
