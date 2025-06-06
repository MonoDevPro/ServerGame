using ServerGame.Domain.Rules;

namespace ServerGame.Domain.ValueObjects;

public readonly record struct Email
{
    public string Value { get; }
    private Email(string value) => Value = value;

    public static Email Create(string input)
        => TryCreate(input, out var vo)
            ? vo
            : throw new ArgumentException("Deve ser um e-mail válido.", nameof(input));

    public static bool TryCreate(string input, out Email result)
    {
        if (EmailRule.IsValidEmail(input))
        {
            result = new Email(input.Trim().ToLowerInvariant());
            return true;
        }
        result = default;
        return false;
    }

    public static implicit operator string(Email e) => e.Value;
    public static implicit operator Email(string s) => Create(s);
}
