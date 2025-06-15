using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Events.Accounts.Base;

/// <summary>
/// Evento de domínio base para todos os eventos relacionados a contas de usuário
/// </summary>
public abstract class AccountDomainEvent(Account account) : DomainEvent
{
    /// <summary>
    /// ID do usuário associado ao evento
    /// </summary>
    public long AccountId { get; } = account.Id;

    public Username Username { get; } = account.Username;
    public Email Email { get; } = account.Email;
}
