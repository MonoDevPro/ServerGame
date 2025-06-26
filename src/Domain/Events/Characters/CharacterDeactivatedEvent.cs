using GameServer.Domain.Events.Characters.Base;

namespace GameServer.Domain.Events.Characters;

/// <summary>
/// Evento disparado quando um personagem é desativado
/// </summary>
public class CharacterDeactivatedEvent(Character character) : CharacterEvent(character);
