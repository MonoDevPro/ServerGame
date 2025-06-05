using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

public class AccountUsernameUpdated : AccountEvent
{
    public string PreviousUsername { get; }
    public string NewUsername { get; }
    
    public AccountUsernameUpdated(
        Account account,
        UsernameVO previousUsername,
        UsernameVO newUsername
        ) : base(account)
    {
        PreviousUsername = previousUsername.Value;
        NewUsername = newUsername.Value;
    }
}
