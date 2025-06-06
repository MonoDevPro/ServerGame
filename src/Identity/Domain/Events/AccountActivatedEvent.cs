using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountActivatedEvent : AccountEvent
{
    public AccountActivatedEvent(
        Account account
        ) : base(account)
    {
    }
}
