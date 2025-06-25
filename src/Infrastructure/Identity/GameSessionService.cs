using System.Diagnostics.Metrics;
using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Interfaces.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GameServer.Infrastructure.Identity;

public class GameSessionService(
    IMemoryCache cache,
    IIdentityService identityService,
    IAccountService accountService,
    ILogger<GameSessionService> logger,
    IMeterFactory meterFactory)
    : IGameSessionService
{
    private const string SESSION_KEY_PREFIX = "game_session:";
    private const string SESSION_DATA_PREFIX = "session_data:";
    private const int DEFAULT_SESSION_MINUTES = 30;

    // Metrics
    private readonly Meter _meter = meterFactory.Create("GameServer.GameSession");
    private readonly Counter<long> _sessionsCreated = meterFactory.Create("GameServer.GameSession").CreateCounter<long>("game_sessions_created", "Count", "Number of game sessions created");
    private readonly Counter<long> _sessionsRenewed = meterFactory.Create("GameServer.GameSession").CreateCounter<long>("game_sessions_renewed", "Count", "Number of game sessions renewed");
    private readonly Counter<long> _sessionsExpired = meterFactory.Create("GameServer.GameSession").CreateCounter<long>("game_sessions_expired", "Count", "Number of game sessions expired");
    private readonly Counter<long> _sessionsRevoked = meterFactory.Create("GameServer.GameSession").CreateCounter<long>("game_sessions_revoked", "Count", "Number of game sessions manually revoked");

    public async Task<bool> IsAccountLoggedInAsync(string userId)
    {
        var cacheKey = $"{SESSION_KEY_PREFIX}{userId}";

        // Verificar no cache primeiro (performance)
        if (cache.TryGetValue(cacheKey, out var cachedAccountId) && cachedAccountId != null)
        {
            return true;
        }

        // Verificar se existe claim AccountId (fallback)
        var hasAccountClaim = await identityService.HasClaimAsync(userId, Domain.Constants.Claims.AccountId);

        if (hasAccountClaim)
        {
            // Restaurar no cache se existe a claim mas não está em cache
            var accountId = await identityService.GetClaimValueAsync(userId, Domain.Constants.Claims.AccountId);
            if (!string.IsNullOrEmpty(accountId))
            {
                await SetAccountSessionAsync(userId, accountId);
                return true;
            }
        }

        return false;
    }

    public Task<bool> IsSessionValidAsync(string userId)
    {
        var cacheKey = $"{SESSION_KEY_PREFIX}{userId}";
        var isValid = cache.TryGetValue(cacheKey, out var cachedAccountId) && cachedAccountId != null;
        return Task.FromResult(isValid);
    }

    public async Task SetAccountSessionAsync(string userId, string accountId, TimeSpan? expiration = null)
    {
        var cacheKey = $"{SESSION_KEY_PREFIX}{userId}";
        var slidingExpiration = expiration ?? TimeSpan.FromMinutes(DEFAULT_SESSION_MINUTES);

        var options = new MemoryCacheEntryOptions
        {
            // Usar SOMENTE SlidingExpiration
            SlidingExpiration = slidingExpiration
        };

        options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
        {
            EvictionCallback = OnSessionExpired
        });

        // Armazenar no cache
        cache.Set(cacheKey, accountId, options);

        // Adicionar/atualizar claim AccountId
        await identityService.AddClaimAsync(userId, Domain.Constants.Claims.AccountId, accountId);

        // Adicionar claim de timestamp da sessão
        await identityService.AddClaimAsync(userId, Domain.Constants.Claims.SessionStartTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

        // Métricas
        _sessionsCreated.Add(1, new KeyValuePair<string, object?>("user_id", userId));

        logger.LogInformation("Game session established for user {UserId} with account {AccountId}, sliding expiration: {SlidingExpiration}",
            userId, accountId, slidingExpiration);
    }

    public Task RefreshSessionAsync(string userId)
    {
        var cacheKey = $"{SESSION_KEY_PREFIX}{userId}";

        if (cache.TryGetValue(cacheKey, out var accountId))
        {
            // Renovar a entrada do cache
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(DEFAULT_SESSION_MINUTES)
            };

            options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
            {
                EvictionCallback = OnSessionExpired
            });

            cache.Set(cacheKey, accountId, options);

            // Métricas
            _sessionsRenewed.Add(1, new KeyValuePair<string, object?>("user_id", userId));

            logger.LogDebug("Game session refreshed for user {UserId}", userId);
        }

        return Task.CompletedTask;
    }

    public Task<TimeSpan?> GetSessionTtlAsync(string userId)
    {
        var cacheKey = $"{SESSION_KEY_PREFIX}{userId}";

        if (cache.TryGetValue(cacheKey, out var _))
        {
            // Como estamos usando SlidingExpiration, não podemos obter o TTL exato
            // Retornamos o tempo padrão de sliding
            return Task.FromResult<TimeSpan?>(TimeSpan.FromMinutes(DEFAULT_SESSION_MINUTES));
        }

        return Task.FromResult<TimeSpan?>(null);
    }

    public async Task RevokeAccountSessionAsync(string userId)
    {
        var cacheKey = $"{SESSION_KEY_PREFIX}{userId}";
        var dataKey = $"{SESSION_DATA_PREFIX}{userId}";

        // Remover do cache
        cache.Remove(cacheKey);
        cache.Remove(dataKey);

        // Remover claims relacionadas à sessão
        await identityService.RemoveClaimAsync(userId, Domain.Constants.Claims.AccountId);
        await identityService.RemoveClaimAsync(userId, Domain.Constants.Claims.SessionStartTime);

        // Métricas
        _sessionsRevoked.Add(1, new KeyValuePair<string, object?>("user_id", userId));

        logger.LogInformation("Game session revoked for user {UserId}", userId);
    }

    public async Task<string?> GetActiveAccountIdAsync(string userId)
    {
        var cacheKey = $"{SESSION_KEY_PREFIX}{userId}";

        if (cache.TryGetValue(cacheKey, out var accountId))
        {
            return accountId as string;
        }

        // Fallback para claim
        return await identityService.GetClaimValueAsync(userId, Domain.Constants.Claims.AccountId);
    }

    public async Task<AccountDto?> GetProfileAsync(string userId)
    {
        try
        {
            return await accountService.GetDtoAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not retrieve profile for user {UserId}", userId);
            return null;
        }
    }

    public Task<Dictionary<string, object>?> GetSessionDataAsync(string userId)
    {
        var dataKey = $"{SESSION_DATA_PREFIX}{userId}";
        cache.TryGetValue(dataKey, out var data);
        return Task.FromResult(data as Dictionary<string, object>);
    }

    public Task UpdateSessionDataAsync(string userId, Dictionary<string, object> data)
    {
        var dataKey = $"{SESSION_DATA_PREFIX}{userId}";
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(DEFAULT_SESSION_MINUTES)
        };

        cache.Set(dataKey, data, options);
        return Task.CompletedTask;
    }

    private void OnSessionExpired(object key, object? value, EvictionReason reason, object? state)
    {
        if (reason == EvictionReason.Expired && key.ToString()?.StartsWith(SESSION_KEY_PREFIX) == true)
        {
            var userId = key.ToString()!.Substring(SESSION_KEY_PREFIX.Length);

            // Métricas
            _sessionsExpired.Add(1, new KeyValuePair<string, object?>("user_id", userId));

            logger.LogInformation("Game session expired for user {UserId}", userId);

            // Remover claims de forma assíncrona (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await identityService.RemoveClaimAsync(userId, Domain.Constants.Claims.AccountId);
                    await identityService.RemoveClaimAsync(userId, Domain.Constants.Claims.SessionStartTime);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error removing claims for expired session of user {UserId}", userId);
                }
            });
        }
    }
}
