using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

public class AccountDomainUsernameUpdatedEvent : AccountDomainEvent
{
    public string PreviousUsername { get; }
    public string NewUsername { get; }
    
    public AccountDomainUsernameUpdatedEvent(
        Account account,
        Username previousUsername,
        Username newUsername
        ) : base(account)
    {
        PreviousUsername = previousUsername.Value;
        NewUsername = newUsername.Value;
    }
}
