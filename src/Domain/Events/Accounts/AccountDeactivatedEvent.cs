using GameServer.Domain.Events.Accounts.Base;

namespace GameServer.Domain.Events.Accounts;

public class AccountDeactivatedEvent(Account account) : AccountEvent(account)
{
}
