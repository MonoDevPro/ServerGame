using GameServer.Application.Session;
using Microsoft.Extensions.Caching.Memory;

namespace GameServer.Infrastructure.Services.Sessions;

/// <summary>
/// Armazena dados arbitrários de sessão (dicionário string→object) em IMemoryCache.
/// </summary>
public class SessionDataStore(IMemoryCache cache) : ISessionDataStore
{
    private const string DATA_KEY_PREFIX = "session_data:";
    private const int    DEFAULT_MINUTES  = 30;
    
    /// <inheritdoc />
    public Task<Dictionary<string, object>?> GetDataAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("userId cannot be null or empty", nameof(userId));

        var key = $"{DATA_KEY_PREFIX}{userId}";
        cache.TryGetValue(key, out Dictionary<string, object>? data);
        return Task.FromResult(data);
    }

    /// <inheritdoc />
    public Task UpdateDataAsync(
        string userId,
        Dictionary<string, object> data,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("userId cannot be null or empty", nameof(userId));
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        var key = $"{DATA_KEY_PREFIX}{userId}";
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(DEFAULT_MINUTES)
        };

        cache.Set(key, data, options);
        return Task.CompletedTask;
    }
}
