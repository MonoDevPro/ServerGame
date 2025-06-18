using Ardalis.GuardClauses;
using ServerGame.Domain.Rules;

namespace ServerGame.Domain.ValueObjects.Accounts;

[Obsolete ("Identity manage email, this is not used anymore")]
public sealed record Email : ValueObject
{
    public string Value { get; }
    private Email(string value) => Value = value;

    public static Email Create(string input)
        => TryCreate(input, out var vo)
            ? Guard.Against.Null(vo, nameof(input))
            : throw new ArgumentException("Deve ser um e-mail válido.", nameof(input));

    public static bool TryCreate(string input, out Email? result)
    {
        if (EmailRule.IsValid(input))
        {
            result = new Email(input.Trim().ToLowerInvariant());
            return true;
        }
        result = null;
        return false;
    }

    public static implicit operator string(Email e) => e.Value;
    public static implicit operator Email(string s) => Create(s);
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
