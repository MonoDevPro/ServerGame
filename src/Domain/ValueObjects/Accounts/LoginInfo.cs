using System.Net;

namespace GameServer.Domain.ValueObjects.Accounts;

public sealed record LoginInfo : ValueObject
{
    public string           LastLoginIp   { get; }
    public DateTimeOffset   LastLoginDate { get; }

    private LoginInfo(string ip, DateTimeOffset date)
        => (LastLoginIp, LastLoginDate) = (ip, date);

    public static LoginInfo Create(string ipAddress, DateTimeOffset loginDate)
        => TryCreate(ipAddress, loginDate, out var vo)
            ? vo!
            : throw new ArgumentException(
                "Informações de login inválidas.", nameof(ipAddress));

    public static bool TryCreate(
        string           ipAddress,
        DateTimeOffset   loginDate,
        out LoginInfo?   result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(ipAddress)) 
            return false;
        if (!IPAddress.TryParse(ipAddress.Trim(), out _)) 
            return false;

        // Normalize para UTC, mas preservando offset (opcional):
        var dto = loginDate.ToOffset(TimeSpan.Zero);

        result = new LoginInfo(ipAddress.Trim(), dto);
        return true;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return LastLoginIp;
        yield return LastLoginDate;
    }
}
