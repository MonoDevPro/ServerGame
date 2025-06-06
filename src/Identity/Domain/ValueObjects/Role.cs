using System;
using System.Collections.Generic;

namespace ServerGame.Domain.ValueObjects;

public sealed record Role
{
    // Definições dos papéis disponíveis
    public static Role Player         => new("player");
    public static Role Vip            => new("vip");
    public static Role Moderator      => new("moderator");
    public static Role GameMaster     => new("gamemaster");
    public static Role Admin          => new("admin");
    public static Role Support        => new("support");
    public static Role ContentCreator => new("contentCreator");

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

    public static implicit operator string(Role r) => r.Value;
    public static implicit operator Role?(string s) => Create(s);
}
