using Ardalis.GuardClauses;
using ServerGame.Domain.Rules;

namespace ServerGame.Domain.ValueObjects.Accounts;

public sealed record Username : ValueObject
{
    public string Value { get; }
    private Username(string value) => Value = value;

    public static Username Create(string input)
        => TryCreate(input, out var vo)
            ? Guard.Against.Null(vo, nameof(input))
            : throw new ArgumentException("Deve ser um username válido (3–20 caracteres: letras, números, '.', '_' ou '-').", nameof(input));

    public static bool TryCreate(string input, out Username? result)
    {
        if (UsernameRule.IsValidUsername(input))
        {
            result = new Username(input.Trim().ToLowerInvariant());
            return true;
        }
        result = null;
        return false;
    }

    public static implicit operator string(Username u) => u.Value;
    public static implicit operator Username(string s) => Create(s);
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
