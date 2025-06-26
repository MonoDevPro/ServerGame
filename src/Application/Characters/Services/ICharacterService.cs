using GameServer.Application.Characters.Queries.Models;
using GameServer.Domain.Entities;
using GameServer.Domain.Enums;

namespace GameServer.Application.Characters.Services;

public interface ICharacterService
{
    Task<bool> ExistsAsync(long characterId, CancellationToken cancellationToken = default);
    Task<List<CharacterSummaryDto>> GetAccountCharactersAsync(long accountId, CancellationToken cancellationToken = default);
    Task<CharacterDto> GetByIdAsync(long characterId, CancellationToken cancellationToken = default);
    Task<Character> CreateAsync(long accountId, string name, CharacterClass characterClass, CancellationToken cancellationToken = default);
    Task<Character?> GetForUpdateAsync(long characterId, CancellationToken cancellationToken = default);
    Task UpdateAsync(CharacterDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(long characterId, CancellationToken cancellationToken = default);
    Task<bool> IsOwnerAsync(long characterId, long accountId, CancellationToken cancellationToken = default);
    Task<bool> CanCreateCharacterAsync(long accountId, CancellationToken cancellationToken = default);
}
