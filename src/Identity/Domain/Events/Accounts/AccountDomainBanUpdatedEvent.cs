using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts;

/// <summary>
/// Evento disparado quando um usuário é banido do sistema
/// </summary>
public class AccountDomainBanUpdatedEvent : AccountDomainEvent
{
    BanInfo BanInfo { get;}  
    public AccountDomainBanUpdatedEvent(Account account, BanInfo banInfo) : base(account)
    {
        BanInfo = banInfo;
    }
}
