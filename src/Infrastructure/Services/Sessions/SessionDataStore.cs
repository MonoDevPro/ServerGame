using GameServer.Application.Session;
using Microsoft.Extensions.Caching.Memory;

namespace GameServer.Infrastructure.Services.Sessions;

public class SessionDataStore : ISessionDataStore
{
    private const string DATA_KEY_PREFIX = "session_data:";
    private const int DEFAULT_MINUTES = 30;
    private readonly IMemoryCache _cache;

    public SessionDataStore(IMemoryCache cache) => _cache = cache;

    public Task<Dictionary<string, object>?> GetDataAsync(string userId)
    {
        var key = $"{DATA_KEY_PREFIX}{userId}";
        _cache.TryGetValue(key, out Dictionary<string, object>? data);
        return Task.FromResult(data);
    }

    public Task UpdateDataAsync(string userId, Dictionary<string, object> data)
    {
        var key = $"{DATA_KEY_PREFIX}{userId}";
        _cache.Set(key, data, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(DEFAULT_MINUTES)
        });
        return Task.CompletedTask;
    }
}
