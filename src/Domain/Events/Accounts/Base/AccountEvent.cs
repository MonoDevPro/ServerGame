namespace GameServer.Domain.Events.Accounts.Base;

/// <summary>
/// Evento de domínio base para todos os eventos relacionados a contas de usuário
/// </summary>
public abstract class AccountEvent(Account account) : DomainEvent
{
    /// <summary>
    /// ID do usuário associado ao evento
    /// </summary>
    public long AccountId { get; } = account.Id;

    public string UserId { get; } = account.CreatedBy ?? string.Empty;
}
