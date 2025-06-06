using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

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
