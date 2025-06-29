using GameServer.Application.Accounts.Services;
using GameServer.Application.Accounts.Services.Current;
using GameServer.Application.Characters.Services;
using GameServer.Application.Characters.Services.Current;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Session;

namespace GameServer.Infrastructure.Services.Characters.Current;

/// <summary>
/// Implementa a lógica de seleção de personagem para o usuário autenticado.
/// </summary>
public class CurrentCharacterSelector(
    IUser user,
    ISessionManager sessionManager,
    ISessionDataStore dataStore,
    ICharacterQueryService characterQuery,
    ICurrentAccountService currentAccount)
    : ICurrentCharacterSelector
{
    private const string SelectedKey = "SelectedCharacterId";

    private string UserId => user.Id ?? throw new InvalidOperationException("User ID is not set");

    /// <inheritdoc />
    public async Task<bool> IsOwnerAsync(long characterId, CancellationToken cancellationToken = default)
    {
        // Garante que o usuário está autenticado
        if (string.IsNullOrWhiteSpace(user.Id))
            return false;

        // Obtém a conta atual
        var accountId = await currentAccount.GetIdAsync(cancellationToken);

        // Verifica se o personagem pertence a esta conta
        return await characterQuery.IsOwnerAsync(characterId, accountId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SelectAsync(long characterId, CancellationToken cancellationToken = default)
    {
        // Garante que o usuário está autenticado e sessão ativa
        if (string.IsNullOrWhiteSpace(user.Id))
            throw new UnauthorizedAccessException("User is not authenticated");

        if (!await sessionManager.HasActiveSessionAsync(UserId))
            throw new InvalidOperationException("No active session for user");

        // Verifica posse do personagem
        if (!await IsOwnerAsync(characterId, cancellationToken))
            throw new UnauthorizedAccessException("Character does not belong to current user");

        // Grava seleção na sessão
        var data = await dataStore.GetDataAsync(UserId, cancellationToken) 
                   ?? new Dictionary<string, object>(capacity: 1);

        data[SelectedKey] = characterId;
        await dataStore.UpdateDataAsync(UserId, data, cancellationToken);
    }
}
