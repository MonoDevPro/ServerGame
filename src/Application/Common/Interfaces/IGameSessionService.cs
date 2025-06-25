using GameServer.Application.Accounts.Queries.Models;

namespace GameServer.Application.Common.Interfaces;

public interface IGameSessionService
{
    Task<bool> IsAccountLoggedInAsync(string userId);
    Task SetAccountSessionAsync(string userId, string accountId, TimeSpan? expiration = null);
    Task RevokeAccountSessionAsync(string userId);
    Task<string?> GetActiveAccountIdAsync(string userId);
    Task<Dictionary<string, object>?> GetSessionDataAsync(string userId);
    Task UpdateSessionDataAsync(string userId, Dictionary<string, object> data);

    // New methods for enhanced session management
    Task<bool> IsSessionValidAsync(string userId);
    Task<AccountDto?> GetProfileAsync(string userId);
    Task<TimeSpan?> GetSessionTtlAsync(string userId);
    Task RefreshSessionAsync(string userId);
}
