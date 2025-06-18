using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events.Accounts.Base;

namespace ServerGame.Domain.Events.Accounts;

/// <summary>
/// Evento disparado quando o tipo de conta de um usuário é alterado
/// </summary>
public class AccountTypeChangedEvent : AccountEvent
{
    /// <summary>
    /// Tipo de conta anterior
    /// </summary>
    public AccountType PreviousAccountType { get; }
    
    /// <summary>
    /// Novo tipo de conta
    /// </summary>
    public AccountType NewAccountType { get; }
    
    /// <summary>
    /// Data de expiração da assinatura (para contas premium/VIP)
    /// </summary>
    public DateTime? SubscriptionExpiresAt { get; }
    
    /// <summary>
    /// Motivo da alteração
    /// </summary>
    public string? Reason { get; }
    
    public AccountTypeChangedEvent(
        Account account,
        AccountType previousAccountType,
        AccountType newAccountType,
        DateTime? subscriptionExpiresAt = null,
        string? reason = null
        ) : base(account)
    {
        PreviousAccountType = previousAccountType;
        NewAccountType = newAccountType;
        SubscriptionExpiresAt = subscriptionExpiresAt;
        Reason = reason;
    }
}
