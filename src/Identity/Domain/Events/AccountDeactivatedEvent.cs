using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountDeactivatedEvent : AccountEvent
{
    public AccountDeactivatedEvent(Account account) : base(account)
    {
    }
}
