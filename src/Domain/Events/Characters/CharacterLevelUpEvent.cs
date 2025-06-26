using GameServer.Domain.Events.Characters.Base;

namespace GameServer.Domain.Events.Characters;

/// <summary>
/// Evento disparado quando um personagem sobe de nível
/// </summary>
public class CharacterLevelUpEvent(Character character, int previousLevel, int newLevel) : CharacterEvent(character)
{
    public int PreviousLevel { get; } = previousLevel;
    public int NewLevel { get; } = newLevel;
}
