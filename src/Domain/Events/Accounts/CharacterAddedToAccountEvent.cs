using GameServer.Domain.Events.Accounts.Base;

namespace GameServer.Domain.Events.Accounts;

/// <summary>
/// Evento disparado quando um personagem é adicionado a uma conta
/// </summary>
public class CharacterAddedToAccountEvent(Account account, Character character) : AccountEvent(account)
{
    public long CharacterId { get; } = character.Id;
    public string CharacterName { get; } = character.Name;
    public string CharacterClass { get; } = character.Class.ToString();
}
