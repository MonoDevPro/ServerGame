using GameServer.Domain.Events.Characters.Base;

namespace GameServer.Domain.Events.Characters;

/// <summary>
/// Evento disparado quando um novo personagem é criado
/// </summary>
public class CharacterCreatedEvent(Character character) : CharacterEvent(character);
