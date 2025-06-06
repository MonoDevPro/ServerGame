namespace ServerGame.Domain.ValueObjects;

public sealed record BanInfo
{
    public BanStatus Status { get; }
    public DateTime? ExpiresAt { get; }
    public string? Reason { get; }
    public long? BannedById { get; }

    private BanInfo(BanStatus status, DateTime? expiresAt, string? reason, long? bannedById)
        => (Status, ExpiresAt, Reason, BannedById) = (status, expiresAt, reason, bannedById);

    public static BanInfo NotBanned => new(BanStatus.NotBanned, null, null, null);

    public static BanInfo? CreateTemporary(string reason, DateTime expiresAt, long bannedById)
        => TryCreateTemporary(reason, expiresAt, bannedById, out var vo)
            ? vo
            : throw new ArgumentException(
                "Banimento temporário inválido.", nameof(reason));

    public static bool TryCreateTemporary(
        string reason,
        DateTime expiresAt,
        long bannedById,
        out BanInfo? result)
    {
        var trimmed = reason?.Trim();
        result = null;
        if (string.IsNullOrWhiteSpace(trimmed)) return false;
        if (expiresAt <= DateTime.UtcNow) return false;

        result = new BanInfo(
            BanStatus.TemporaryBan,
            expiresAt,
            trimmed,
            bannedById);
        return true;
    }

    public static BanInfo? CreatePermanent(string reason, long bannedById)
        => TryCreatePermanent(reason, bannedById, out var vo)
            ? vo
            : throw new ArgumentException(
                "Banimento permanente inválido.", nameof(reason));

    public static bool TryCreatePermanent(
        string reason,
        long bannedById,
        out BanInfo? result)
    {
        var trimmed = reason?.Trim();
        result = null;
        if (string.IsNullOrWhiteSpace(trimmed)) return false;

        result = new BanInfo(
            BanStatus.PermanentBan,
            null,
            trimmed,
            bannedById);
        return true;
    }

    public bool IsActive() => Status switch
    {
        BanStatus.PermanentBan => true,
        BanStatus.TemporaryBan => ExpiresAt > DateTime.UtcNow,
        _ => false
    };    }
