using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

/// <summary>
/// Evento disparado quando um novo usuário é registrado no sistema
/// </summary>
public class AccountCreatedEvent : AccountEvent
{
    /// <summary>
    /// Email do usuário registrado
    /// </summary>
    public string Email { get; }
    
    /// <summary>
    /// Nome de usuário escolhido
    /// </summary>
    public string Username { get; }
    
    public AccountCreatedEvent(Account account) : base(account)
    {
        Email = account.Email.Value;
        Username = account.Username.Value;
    }
}
