using Ardalis.GuardClauses;

namespace ServerGame.Domain.ValueObjects.Accounts;

[Obsolete ("Identity manage username or email, this is not used anymore")]
public sealed record UsernameOrEmail : ValueObject
{
    public Username? Username { get; } = null;
    public Email? Email { get; } = null;

    private UsernameOrEmail(string normalized)
    {
        if (Email.TryCreate(normalized, out var e))
            Email = e;
        else if (Username.TryCreate(normalized, out var u))
            Username = u;
        else
            throw new ArgumentException(
                $"Deve ser um e-mail ou username válido: '{normalized}'.",
                nameof(normalized));
    }

    public static UsernameOrEmail Create(string input)
        => TryCreate(input, out var vo)
            ? Guard.Against.Null(vo)
            : throw new ArgumentException(
                $"'{input}' não é um e‑mail ou username válido.",
                nameof(input));

    public static bool TryCreate(string input, out UsernameOrEmail? result)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            result = null;
            return false;
        }
        
        var normalized = input.Trim().ToLowerInvariant();
        
        try
        {
            result = new UsernameOrEmail(normalized);
            return true;
        }
        catch (ArgumentException)
        {
            // Invalid format, return null
            result = null;
            return false;
        }
    }

    public static implicit operator string(UsernameOrEmail u) => 
        u.Username?.Value ?? u.Email?.Value ?? throw new InvalidOperationException(
            "UsernameOrEmail must contain either a Username or an Email.");
    public static implicit operator UsernameOrEmail(string s) => Create(s);
    public static implicit operator UsernameOrEmail(Username u) => new(u.Value);
    public static implicit operator UsernameOrEmail(Email e) => new(e.Value);
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Username?.Value ?? Email?.Value ?? string.Empty;
    }
}
