using GameServer.Shared.Domain.Events;

namespace ServerGame.Domain.Events.Base;

/// <summary>
/// Evento de domínio base para todos os eventos relacionados a contas de usuário
/// </summary>
public abstract class AccountEvent : DomainEvent
{
    /// <summary>
    /// ID do usuário associado ao evento
    /// </summary>
    public long AccountId { get; }
    
    protected AccountEvent(Account account) : base()
    {
        AccountId = account.Id;
    }
}
