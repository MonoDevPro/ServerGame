
namespace GameServer.Application.Characters.Services;

/// <summary>
/// Serviço que gerencia o personagem atualmente selecionado em uma sessão de jogo.
/// </summary>
public interface ISelectedCharacterService
{
    /// <summary>
    /// Verifica se há um personagem selecionado na sessão do usuário.
    /// </summary>
    Task<bool> HasSelectedAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o ID do personagem selecionado para o usuário.
    /// </summary>
    Task<long?> GetSelectedIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Define o personagem selecionado na sessão.
    /// </summary>
    Task SetSelectedAsync(string userId, long characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a seleção de personagem da sessão.
    /// </summary>
    Task ClearSelectedAsync(string userId, CancellationToken cancellationToken = default);
}
