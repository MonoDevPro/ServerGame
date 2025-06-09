using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

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
