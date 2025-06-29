using GameServer.Application.Characters.Queries.Models;
using GameServer.Domain.Entities;
using GameServer.Domain.Enums;

namespace GameServer.Application.Characters.Services;

public interface ICharacterCommandService
{
    /// <summary>
    /// Cria um novo personagem para determinada conta.
    /// </summary>
    Task<Character> CreateAsync(long accountId, string name, CharacterClass characterClass, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aplica alterações baseadas em um DTO.
    /// </summary>
    Task UpdateAsync(CharacterDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove (soft‑delete ou hard‑delete) um personagem.
    /// </summary>
    Task DeleteAsync(long characterId, CancellationToken cancellationToken = default);
    
    Task PurgeAsync(long characterId, CancellationToken cancellationToken = default);
}
