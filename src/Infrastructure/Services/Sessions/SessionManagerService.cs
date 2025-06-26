
// 1. SessionManagerService: gerencia lifecycle da sessão (cache + eviction)

using System.Diagnostics.Metrics;
using GameServer.Application.Session;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GameServer.Infrastructure.Services.Sessions;

public class SessionManagerService : ISessionManager
{
    private const string SESSION_KEY_PREFIX = "game_session:";
    private const int DEFAULT_MINUTES = 30;
    private readonly IMemoryCache _cache;
    private readonly Counter<long> _created;
    private readonly Counter<long> _renewed;
    private readonly Counter<long> _expired;
    private readonly Counter<long> _revoked;
    private readonly SessionExpirationService _expiration;
    private readonly ILogger<SessionManagerService> _logger;

    public SessionManagerService(
        IMemoryCache cache,
        SessionExpirationService expiration,
        ILogger<SessionManagerService> logger,
        IMeterFactory meterFactory)
    {
        _cache = cache;
        _expiration = expiration;
        _logger = logger;
        var meter = meterFactory.Create("GameServer.SessionManager");
        _created = meter.CreateCounter<long>("sessions_created");
        _renewed = meter.CreateCounter<long>("sessions_renewed");
        _expired = meter.CreateCounter<long>("sessions_expired");
        _revoked = meter.CreateCounter<long>("sessions_revoked");
    }

    public Task<bool> HasActiveSessionAsync(string userId)
    {
        var key = GetKey(userId);
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public Task RefreshSessionAsync(string userId)
    {
        var key = GetKey(userId);
        if (_cache.TryGetValue(key, out GameSessionData? data) && data != null)
        {
            data.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(DEFAULT_MINUTES);
            _cache.Set(key, data, CreateOptions());
            _renewed.Add(1);
            
            _logger.LogDebug("Session renewed for {UserId}", userId);
        }

        return Task.CompletedTask;
    }

    public Task SetSessionAsync(string userId, long accountId, TimeSpan? expiration = null)
    {
        var key = GetKey(userId);
        var ttl = expiration ?? TimeSpan.FromMinutes(DEFAULT_MINUTES);
        var data = new GameSessionData(userId, accountId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.Add(ttl));
        _cache.Set(key, data, CreateOptions());
        _created.Add(1);
        _logger.LogInformation("Session created for {UserId}", userId);
        return Task.CompletedTask;
    }

    public Task<TimeSpan?> GetSessionTtlAsync(string userId)
    {
        var key = GetKey(userId);
        if (_cache.TryGetValue(key, out GameSessionData? data) && data != null)
        {
            var rem = data.ExpiresAt - DateTimeOffset.UtcNow;
            return Task.FromResult<TimeSpan?>(rem > TimeSpan.Zero ? rem : TimeSpan.Zero);
        }

        return Task.FromResult<TimeSpan?>(null);
    }

    public Task RevokeSessionAsync(string userId)
    {
        var key = GetKey(userId);
        _cache.Remove(key);
        _revoked.Add(1);
        _logger.LogInformation("Session revoked for {UserId}", userId);
        return Task.CompletedTask;
    }

    private MemoryCacheEntryOptions CreateOptions() => new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(DEFAULT_MINUTES),
        PostEvictionCallbacks = { new PostEvictionCallbackRegistration { EvictionCallback = OnEvicted } }
    };

    private void OnEvicted(object? key, object? value, EvictionReason reason, object? state)
    {
        if (reason == EvictionReason.Expired && value is GameSessionData data)
        {
            _expired.Add(1);
            _logger.LogInformation("Session expired for {UserId}", data.UserId);
            _expiration.EnqueueExpiration(data.UserId, "expired");
        }
    }

    private static string GetKey(string userId) => $"{SESSION_KEY_PREFIX}{userId}";

    // Internal DTO for cache storage
    private sealed class GameSessionData(
        string userId,
        long accountId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        public string UserId { get; init; } = userId;
        public long AccountId { get; init; } = accountId;
        public DateTimeOffset CreatedAt { get; init; } = createdAt;
        public DateTimeOffset ExpiresAt { get; set; } = expiresAt;
    }
}
