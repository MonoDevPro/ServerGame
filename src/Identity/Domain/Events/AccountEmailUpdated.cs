using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountEmailUpdated : AccountEvent
{
    public string PreviousEmail { get; }
    public string NewEmail { get; }
    
    public AccountEmailUpdated(
        Account account,
        EmailVO previousEmail,
        EmailVO newEmail
        ) : base(account)
    {
        PreviousEmail = previousEmail.Value;
        NewEmail = newEmail.Value;
    }
}
