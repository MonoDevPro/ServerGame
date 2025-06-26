using GameServer.Application.Characters.Queries.Models;
using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Domain.Enums;

namespace GameServer.Infrastructure.Services.Characters;

/// <summary>
/// Serviço que representa o personagem atual do usuário autenticado.
/// </summary>
public class CurrentCharacterService(
    IUser user,
    ISelectedCharacterService selection,
    ICharacterService characters)
    : ICurrentCharacterService
{
    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
            return false;

        var selectedId = await selection.GetSelectedIdAsync(user.Id, cancellationToken);
        return selectedId.HasValue 
               && await characters.ExistsAsync(selectedId.Value, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CharacterDto> GetDtoAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
            throw new NotFoundException("No user authenticated", nameof(user.Id));

        var selectedId = await selection.GetSelectedIdAsync(user.Id, cancellationToken)
                         ?? throw new NotFoundException("No character selected", nameof(CurrentCharacterService));

        return await characters.GetByIdAsync(selectedId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
            throw new NotFoundException("No user authenticated", nameof(user.Id));

        var selectedId = await selection.GetSelectedIdAsync(user.Id, cancellationToken);
        if (!selectedId.HasValue)
            return; // nada a fazer

        // Remove o personagem e limpa a seleção
        await characters.DeleteAsync(selectedId.Value, cancellationToken);
        await selection.ClearSelectedAsync(user.Id, cancellationToken);
    }
}
