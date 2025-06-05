using GameServer.Shared.Domain.ValueObjects;

namespace ServerGame.Domain.ValueObjects;

public sealed class LoginInfoVO : ValueObject
{
    public string LastLoginIp { get; }
    public DateTime LastLoginDate { get; private set; } = default!;

    private LoginInfoVO(string lastLoginIp, DateTime lastLoginDate)
    {
        LastLoginIp = lastLoginIp;
        LastLoginDate = lastLoginDate;
    }

    public static LoginInfoVO Create(string ipAddress, DateTime lastLoginDate)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP não pode ser vazio", nameof(ipAddress));

        return new LoginInfoVO(ipAddress, lastLoginDate);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return LastLoginIp;
        yield return LastLoginDate;
    }
}
