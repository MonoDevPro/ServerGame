using GameServer.Domain.Events.Accounts.Base;
using GameServer.Domain.ValueObjects.Accounts;

namespace GameServer.Domain.Events.Accounts;

/// <summary>
/// Evento disparado quando um usuário é banido do sistema
/// </summary>
public class AccountBanUpdatedEvent : AccountEvent
{
    BanInfo BanInfo { get;}  
    public AccountBanUpdatedEvent(Account account, BanInfo banInfo) : base(account)
    {
        BanInfo = banInfo;
    }
}
