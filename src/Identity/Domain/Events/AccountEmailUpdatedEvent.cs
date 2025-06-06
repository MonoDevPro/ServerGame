using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountEmailUpdatedEvent : AccountEvent
{
    public Email PreviousEmail { get; }
    public Email NewEmail { get; }
    
    public AccountEmailUpdatedEvent(
        Account account,
        Email previousEmail,
        Email newEmail
        ) : base(account)
    {
        PreviousEmail = previousEmail.Value;
        NewEmail = newEmail.Value;
    }
}
