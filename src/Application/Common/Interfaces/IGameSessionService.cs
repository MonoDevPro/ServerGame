namespace GameServer.Application.Common.Interfaces;

public interface IGameSessionService
{
    Task<bool> IsAccountLoggedInAsync(string userId);
    Task SetAccountSessionAsync(string userId, string accountId, TimeSpan? expiration = null);
    Task RevokeAccountSessionAsync(string userId);
    Task<string?> GetActiveAccountIdAsync(string userId);
    Task<Dictionary<string, object>?> GetSessionDataAsync(string userId);
    Task UpdateSessionDataAsync(string userId, Dictionary<string, object> data);
}
