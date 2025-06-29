using GameServer.Application.Characters.Queries.Models;
using GameServer.Application.Characters.Services;
using GameServer.Application.Characters.Services.Current;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Session;

namespace GameServer.Infrastructure.Services.Characters.Current;

/// <summary>
/// Implementa a seleção de personagem no escopo de sessão.
/// </summary>
public class CurrentCharacterSelection(
    IUser user,
    ICharacterQueryService query,
    ICharacterCommandService command,
    ISessionManager sessionManager,
    ISessionDataStore dataStore)
    : ICurrentCharacterSelection
{
    
    private const string SelectedKey = "SelectedCharacterId";
    private string UserId => user.Id ?? throw new InvalidOperationException("User ID is not set");
    
    #region Helpers
    private Task EnsureUserAuthenticatedAsync()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            throw new UnauthorizedAccessException("User is not authenticated");
        return Task.CompletedTask;
    }
    private async Task EnsureSessionActiveAsync()
    {
        await EnsureUserAuthenticatedAsync();
        if (!await sessionManager.HasActiveSessionAsync(UserId))
            throw new InvalidOperationException("No active session for user");
    }
    private async Task<long> GetSelectedCharacterIdOrThrowAsync(CancellationToken cancellationToken)
    {
        await EnsureSessionActiveAsync();

        var data = await dataStore.GetDataAsync(UserId, cancellationToken);
        if (data == null
            || !data.TryGetValue(SelectedKey, out var raw)
            || raw is not long characterId
            || characterId <= 0)
        {
            throw new NotFoundException("No character selected", nameof(characterId));
        }

        return characterId;
    }
    #endregion
    
    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await GetSelectedCharacterIdOrThrowAsync(cancellationToken);
            return true;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }
    
    public Task<long> GetIdAsync(CancellationToken cancellationToken = default)
        => GetSelectedCharacterIdOrThrowAsync(cancellationToken);
        
    public async Task<CharacterDto> GetDtoAsync(CancellationToken cancellationToken = default)
    {
        var characterId = await GetSelectedCharacterIdOrThrowAsync(cancellationToken);
        return await query.GetDtoAsync(characterId, cancellationToken);
    }

    public async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        var characterId = await GetSelectedCharacterIdOrThrowAsync(cancellationToken);
        await command.PurgeAsync(characterId, cancellationToken);
        await LogoutAsync(cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await EnsureUserAuthenticatedAsync();

        var data = await dataStore.GetDataAsync(UserId, cancellationToken);
        if (data?.Remove(SelectedKey) == true)
            await dataStore.UpdateDataAsync(UserId, data, cancellationToken);
    }
}
