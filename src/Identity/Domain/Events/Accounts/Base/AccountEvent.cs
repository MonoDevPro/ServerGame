using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts.Base;

/// <summary>
/// Evento de domínio base para todos os eventos relacionados a contas de usuário
/// </summary>
public abstract class AccountEvent : DomainEvent
{
    /// <summary>
    /// ID do usuário associado ao evento
    /// </summary>
    public long AccountId { get; }
    public Username Username { get; }
    public Email Email { get; } 
    
    protected AccountEvent(Account account) : base()
    {
        AccountId = account.Id;
        Username = account.Username;
        Email = account.Email;
    }
}
