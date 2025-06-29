
using GameServer.Application.Characters.Queries.Models;

namespace GameServer.Application.Characters.Services.Current;

/// <summary>
/// Serviço que gerencia o personagem atualmente selecionado em uma sessão de jogo.
/// </summary>
public interface ICurrentCharacterSelection
{
    /// <summary>
    /// Verifica se há um personagem selecionado na sessão do usuário.
    /// </summary>
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o ID do personagem selecionado para o usuário.
    /// </summary>
    Task<long> GetIdAsync(CancellationToken cancellationToken = default);
    
    Task<CharacterDto> GetDtoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a seleção de personagem da sessão.
    /// </summary>
    Task PurgeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Logout do personagem selecionado.
    /// </summary>
    Task LogoutAsync(CancellationToken cancellationToken = default);
}
