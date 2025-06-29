namespace GameServer.Application.Session;

public interface ISessionDataStore
{
    Task<Dictionary<string, object>?> GetDataAsync(string userId, CancellationToken cancellationToken = default);
    Task UpdateDataAsync(string userId, Dictionary<string, object> data, CancellationToken cancellationToken = default);
}
