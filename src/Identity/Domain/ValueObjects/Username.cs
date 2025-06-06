using ServerGame.Domain.Rules;

namespace ServerGame.Domain.ValueObjects;

public readonly record struct Username
{
    public string Value { get; }
    private Username(string value) => Value = value;

    public static Username Create(string input)
        => TryCreate(input, out var vo)
            ? vo
            : throw new ArgumentException("Deve ser um username válido (3–20 caracteres: letras, números, '.', '_' ou '-').", nameof(input));

    public static bool TryCreate(string input, out Username result)
    {
        if (UsernameRule.IsValidUsername(input))
        {
            result = new Username(input.Trim().ToLowerInvariant());
            return true;
        }
        result = default;
        return false;
    }

    public static implicit operator string(Username u) => u.Value;
    public static implicit operator Username(string s) => Create(s);
}
