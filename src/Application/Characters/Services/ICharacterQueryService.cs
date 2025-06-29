using GameServer.Application.Characters.Queries.Models;
using GameServer.Domain.Entities;

namespace GameServer.Application.Characters.Services;

public interface ICharacterQueryService
{
    /// <summary>
    /// Diz se o personagem existe no sistema.
    /// </summary>
    Task<bool> ExistsAsync(long characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista os personagens (resumo) de uma conta.
    /// </summary>
    Task<List<CharacterSummaryDto>> GetAccountCharactersAsync(long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca o DTO completo de um personagem.
    /// </summary>
    Task<CharacterDto> GetDtoAsync(long characterId, CancellationToken cancellationToken = default);
    
    Task<Character> GetByIdAsync(long characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se o personagem pertence a certa conta.
    /// </summary>
    Task<bool> IsOwnerAsync(long characterId, long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Diz se aquela conta ainda pode criar personagens.
    /// </summary>
    Task<bool> CanCreateCharacterAsync(long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se o nome do personagem é único no sistema.
    /// </summary>
    Task<bool> IsCharacterNameUniqueAsync(string name, CancellationToken cancellationToken = default);
}
