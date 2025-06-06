using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

/// <summary>
/// Evento disparado quando um novo usuário é registrado no sistema
/// </summary>
public class AccountCreatedEvent : AccountEvent
{
    public AccountCreatedEvent(Account account) : base(account)
    {
    }
}
