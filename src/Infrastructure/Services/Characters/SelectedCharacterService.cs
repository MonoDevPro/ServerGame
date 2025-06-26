using GameServer.Application.Characters.Services;
using GameServer.Application.Session;

namespace GameServer.Infrastructure.Services.Characters;

public class SelectedCharacterService(
    ISessionManager sessionManager,
    ISessionDataStore dataStore)
    : ISelectedCharacterService
{
    private const string SelectedKey = "SelectedCharacterId";

    public async Task<bool> HasSelectedAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (!await sessionManager.HasActiveSessionAsync(userId))
            return false;

        var data = await dataStore.GetDataAsync(userId);
        return data != null 
            && data.TryGetValue(SelectedKey, out var raw) 
            && raw is long;
    }

    public async Task<long?> GetSelectedIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var data = await dataStore.GetDataAsync(userId);
        if (data != null 
         && data.TryGetValue(SelectedKey, out var raw) 
         && raw is long id)
        {
            return id;
        }

        return null;
    }

    public async Task SetSelectedAsync(string userId, long characterId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        if (characterId <= 0)
            throw new ArgumentException("Character ID must be positive", nameof(characterId));

        if (!await sessionManager.HasActiveSessionAsync(userId))
            throw new InvalidOperationException("No active session found for user");

        var data = await dataStore.GetDataAsync(userId) 
                   ?? new Dictionary<string, object>();

        data[SelectedKey] = characterId;
        await dataStore.UpdateDataAsync(userId, data);
    }

    public async Task ClearSelectedAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return;

        var data = await dataStore.GetDataAsync(userId);
        if (data != null && data.Remove(SelectedKey))
        {
            await dataStore.UpdateDataAsync(userId, data);
        }
    }
}
