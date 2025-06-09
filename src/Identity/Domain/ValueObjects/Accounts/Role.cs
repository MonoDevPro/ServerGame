namespace ServerGame.Domain.ValueObjects.Accounts;

public sealed record Role: ValueObject
{
    // Definições dos papéis disponíveis
    public static Role Player         => new(Constants.Roles.Player);
    public static Role Vip            => new(Constants.Roles.Vip);
    public static Role Moderator      => new(Constants.Roles.Moderator);
    public static Role GameMaster     => new(Constants.Roles.GameMaster);
    public static Role Admin          => new(Constants.Roles.Administrator);
    public static Role Support        => new(Constants.Roles.Support);
    public static Role ContentCreator => new(Constants.Roles.ContentCreator);

    // Internamente mantemos a lista de valores válidos
    private static readonly HashSet<string> ValidRoles = new(
        StringComparer.OrdinalIgnoreCase)
    {
        Player.Value,
        Vip.Value,
        Moderator.Value,
        GameMaster.Value,
        Admin.Value,
        Support.Value,
        ContentCreator.Value
    };

    private static readonly HashSet<string> ChatModerators = new(
        StringComparer.OrdinalIgnoreCase)
    {
        Moderator.Value,
        GameMaster.Value,
        Admin.Value
    };

    public string Value { get; }

    // Construtor privado centralizado: assume input já validado
    private Role(string value) => Value = value.ToLowerInvariant();

    public static Role? Create(string role)
        => TryCreate(role, out var vo)
            ? vo
            : throw new ArgumentException(
                $"Função inválida: '{role}'", nameof(role));

    public static bool TryCreate(string role, out Role? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(role))
            return false;

        var trimmed = role.Trim();
        if (!ValidRoles.Contains(trimmed))
            return false;

        result = new Role(trimmed);
        return true;
    }

    public bool CanModerateChat() => ChatModerators.Contains(Value);

    public bool CanManageAccounts() => this == Admin;

    public override string ToString() => Value;
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Role r) => r.Value;
    public static implicit operator Role?(string s) => Create(s);
}
