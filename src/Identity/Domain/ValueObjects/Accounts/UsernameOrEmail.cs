using Ardalis.GuardClauses;

namespace ServerGame.Domain.ValueObjects.Accounts;

public sealed record UsernameOrEmail : ValueObject
{
    public string Value { get; }

    private UsernameOrEmail(string value) => Value = value;

    public bool IsEmail    => Email.TryCreate(Value, out _);
    public bool IsUsername => Username.TryCreate(Value, out _);

    public static UsernameOrEmail Create(string input)
        => TryCreate(input, out var vo)
            ? Guard.Against.Null(vo)
            : throw new ApplicationException(
                $"Deve ser um e-mail ou username válido. {nameof(input)}");

    public static bool TryCreate(string input, out UsernameOrEmail? result)
    {
        var normalized = input?.Trim().ToLowerInvariant() ?? string.Empty;

        if (Email.TryCreate(normalized, out _) 
            || Username.TryCreate(normalized, out _))
        {
            result = new UsernameOrEmail(normalized);
            return true;
        }

        result = null;
        return false;
    }

    public static implicit operator string(UsernameOrEmail u) => u.Value;
    public static implicit operator UsernameOrEmail(string s) => Create(s);
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
