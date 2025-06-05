using ServerGame.Domain.Events.Base;

namespace ServerGame.Domain.Events;

/// <summary>
/// Evento disparado quando um usuário é banido do sistema
/// </summary>
public class AccountBanUpdatedEvent : AccountEvent
{
    public BanStatus BanType { get; }
    public DateTime? BanExpiresAt { get; }
    public string? BanReason { get; }
    public long? BannedByUserId { get; }
    
    // Construtor usando o VO
    public AccountBanUpdatedEvent(long accountId, BanInfoVO banInfo) : base(accountId)
    {
        BanType = banInfo.Status;
        BanExpiresAt = banInfo.ExpiresAt;
        BanReason = banInfo.Reason;
        BannedByUserId = banInfo.BannedById;
    }
}
