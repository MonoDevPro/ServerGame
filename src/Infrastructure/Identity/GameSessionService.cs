using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Interfaces.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GameServer.Infrastructure.Identity;

public class GameSessionService(
    IMemoryCache cache,
    IIdentityService identityService,
    ILogger<GameSessionService> logger)
    : IGameSessionService
{
    private const string SESSION_KEY_PREFIX = "game_session:";
    private const string SESSION_DATA_PREFIX = "session_data:";

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

    public async Task SetAccountSessionAsync(string userId, string accountId, TimeSpan? expiration = null)
    {
        var cacheKey = $"{SESSION_KEY_PREFIX}{userId}";
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(8),
            SlidingExpiration = TimeSpan.FromMinutes(30)
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

        logger.LogInformation("Game session established for user {UserId} with account {AccountId}", userId, accountId);
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
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
        
        cache.Set(dataKey, data, options);
        return Task.CompletedTask;
    }

    private void OnSessionExpired(object key, object? value, EvictionReason reason, object? state)
    {
        if (reason == EvictionReason.Expired && key.ToString()?.StartsWith(SESSION_KEY_PREFIX) == true)
        {
            var userId = key.ToString()!.Substring(SESSION_KEY_PREFIX.Length);
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
