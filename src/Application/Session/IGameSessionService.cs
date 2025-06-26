using GameServer.Application.Accounts.Queries.Models;

namespace GameServer.Application.Session;

public interface IGameSessionService
{
    Task<bool> IsAccountLoggedInAsync(string userId);
    Task SetAccountSessionAsync(string userId, long accountId, TimeSpan? expiration = null);
    Task RevokeAccountSessionAsync(string userId);
    Task<long?> GetActiveAccountIdAsync(string userId);
    Task<Dictionary<string, object>?> GetSessionDataAsync(string userId);
    Task UpdateSessionDataAsync(string userId, Dictionary<string, object> data);

    // Character selection methods
    Task SetSelectedCharacterAsync(string userId, long characterId);
    Task<long?> GetSelectedCharacterIdAsync(string userId);
    Task ClearSelectedCharacterAsync(string userId);

    // New methods for enhanced session management
    Task<bool> IsSessionValidAsync(string userId);
    Task<AccountDto?> GetProfileAsync(string userId);
    Task<TimeSpan?> GetSessionTtlAsync(string userId);
    Task RefreshSessionAsync(string userId);
}
