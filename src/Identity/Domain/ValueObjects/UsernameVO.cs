using System.Text.RegularExpressions;
using GameServer.Shared.Domain.ValueObjects;

namespace ServerGame.Domain.ValueObjects;

public sealed class UsernameVO : ValueObject
{
    private const int MinLength = 3;
    private const int MaxLength = 20;

    public string Value { get; private set; }

    private UsernameVO(string value)
    {
        Value = value;
    }

    public static UsernameVO Create(string username)
    {
        Validate(username);
        return new UsernameVO(username.Trim().ToLowerInvariant());
    }

    public static void Validate(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username));

        if (username.Length < MinLength)
            throw new ArgumentException($"Nome de usuário deve ter no mínimo {MinLength} caracteres");

        if (username.Length > MaxLength)
            throw new ArgumentException($"Nome de usuário não pode exceder {MaxLength} caracteres");

        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            throw new ArgumentException("Nome de usuário deve conter apenas letras, números e underscore");
    }

    public override string ToString() => Value;
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
