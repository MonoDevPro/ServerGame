using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

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
