namespace GameServer.Application.Session;

public interface ISessionDataStore
{
    Task<Dictionary<string, object>?> GetDataAsync(string userId);
    Task UpdateDataAsync(string userId, Dictionary<string, object> data);
}
