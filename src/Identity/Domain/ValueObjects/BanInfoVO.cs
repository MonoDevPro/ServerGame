using GameServer.Shared.Domain.ValueObjects;

namespace ServerGame.Domain.ValueObjects;

public sealed class BanInfoVO : ValueObject
{
    public BanStatus Status { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string? Reason { get; private set; }
    public long? BannedById { get; private set; }
    
    private BanInfoVO() { }

    private BanInfoVO(BanStatus status, DateTime? expiresAt, string? reason, long? bannedById)
    {
        Status = status;
        ExpiresAt = expiresAt;
        Reason = reason;
        BannedById = bannedById;
    }

    public static BanInfoVO NotBanned() => 
        new BanInfoVO(BanStatus.NotBanned, null, null, null);

    public static BanInfoVO TemporaryBan(string reason, DateTime expiresAt, long bannedById)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Data de expiração deve ser no futuro");

        return new BanInfoVO(BanStatus.TemporaryBan, expiresAt, reason, bannedById);
    }

    public static BanInfoVO PermanentBan(string reason, long bannedById)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        return new BanInfoVO(BanStatus.PermanentBan, null, reason, bannedById);
    }

    public bool IsActive() => 
        Status != BanStatus.NotBanned && 
        (Status == BanStatus.PermanentBan || ExpiresAt > DateTime.UtcNow);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Status;
        yield return ExpiresAt ?? DateTime.MinValue; // Evita null no hash
        yield return Reason ?? string.Empty; // Evita null no hash
        yield return BannedById ?? 0; // Evita null no hash
    }
}
