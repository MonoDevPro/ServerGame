using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountUsernameUpdatedEvent : AccountEvent
{
    public string PreviousUsername { get; }
    public string NewUsername { get; }
    
    public AccountUsernameUpdatedEvent(
        Account account,
        Username previousUsername,
        Username newUsername
        ) : base(account)
    {
        PreviousUsername = previousUsername.Value;
        NewUsername = newUsername.Value;
    }
}
