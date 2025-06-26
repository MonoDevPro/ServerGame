namespace GameServer.Application.Session;

public interface ISessionManager
{
    Task<bool> HasActiveSessionAsync(string userId);
    Task SetSessionAsync(string userId, long accountId, TimeSpan? expiration = null);
    Task RefreshSessionAsync(string userId);
    Task RevokeSessionAsync(string userId);
    Task<TimeSpan?> GetSessionTtlAsync(string userId);
}
