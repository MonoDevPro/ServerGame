namespace GameServer.Domain.Events.Characters.Base;

/// <summary>
/// Evento de domínio base para todos os eventos relacionados a personagens
/// </summary>
public abstract class CharacterEvent(Character character) : DomainEvent
{
    /// <summary>
    /// ID do personagem associado ao evento
    /// </summary>
    public long CharacterId { get; } = character.Id;

    /// <summary>
    /// ID da conta proprietária do personagem
    /// </summary>
    public long AccountId { get; } = character.AccountId;

    /// <summary>
    /// Nome do personagem
    /// </summary>
    public string CharacterName { get; } = character.Name;
}
