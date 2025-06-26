using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Characters.Queries.Models;
using GameServer.Domain.Entities;
using GameServer.Domain.Enums;

namespace GameServer.Application.Characters.Services;

public interface ICurrentCharacterService
{
    /// <summary>
    /// Retorna true se o usuário autenticado já possui uma personagem no domínio de jogo.
    /// </summary>
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retorna o DTO do personagem do usuário autenticado.
    /// </summary>
    Task<CharacterDto> GetDtoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove o personagem do usuário autenticado.
    /// </summary>
    Task PurgeAsync(CancellationToken cancellationToken = default);
}
