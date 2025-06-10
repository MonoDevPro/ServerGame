using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;

namespace ServerGame.Domain.Events.Accounts;

/// <summary>
/// Evento disparado quando um novo usuário é registrado no sistema
/// </summary>
public class AccountDomainCreatedEvent : AccountDomainEvent
{
    public AccountDomainCreatedEvent(Account account) : base(account)
    {
    }
}
