using System.Diagnostics.Metrics;
using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces.Identity;
using GameServer.Application.Session;
using GameServer.Infrastructure.Services;
using GameServer.Infrastructure.Services.Sessions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GameServer.Infrastructure.Identity;

/// <summary>
/// Serviço de sessão de jogo com fonte única de verdade baseada em cache.
/// Elimina fallbacks automáticos por claims e garante consistência de dados.
/// </summary>
public class GameSessionService : IGameSessionService
{
    private const string SESSION_KEY_PREFIX = "game_session:";
    private const int DEFAULT_SESSION_MINUTES = 30;

    // Dependencies
    private readonly IMemoryCache _cache;
    private readonly IIdentityService _identityService;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly SessionExpirationService _expirationService;
    private readonly ILogger<GameSessionService> _logger;

    // Metrics - unificados num único Meter
    private readonly Counter<long> _sessionsCreated;
    private readonly Counter<long> _sessionsRenewed;
    private readonly Counter<long> _sessionsExpired;
    private readonly Counter<long> _sessionsRevoked;

    public GameSessionService(
        IMemoryCache cache,
        IIdentityService identityService,
        ICurrentAccountService currentAccountService,
        SessionExpirationService expirationService,
        ILogger<GameSessionService> logger,
        IMeterFactory meterFactory)
    {
        this._cache = cache;
        this._identityService = identityService;
        this._currentAccountService = currentAccountService;
        this._expirationService = expirationService;
        this._logger = logger;

        Meter meter = meterFactory.Create("GameServer.GameSession");
        _sessionsCreated = meter.CreateCounter<long>("game_sessions_created", "sessions", "Number of game sessions created");
        _sessionsRenewed = meter.CreateCounter<long>("game_sessions_renewed", "sessions", "Number of game sessions renewed");
        _sessionsExpired = meter.CreateCounter<long>("game_sessions_expired", "sessions", "Number of game sessions expired");
        _sessionsRevoked = meter.CreateCounter<long>("game_sessions_revoked", "sessions", "Number of game sessions manually revoked");
    }

    /// <summary>
    /// Verifica se há uma sessão ativa para o usuário.
    /// Fonte única de verdade - apenas cache, sem fallbacks.
    /// </summary>
    public Task<bool> HasActiveSessionAsync(string userId)
    {
        var cacheKey = GetSessionKey(userId);
        var hasSession = _cache.TryGetValue(cacheKey, out var sessionData) && sessionData is GameSessionData;
        return Task.FromResult(hasSession);
    }

    /// <summary>
    /// Alias para compatibilidade com interface existente.
    /// </summary>
    public Task<bool> IsAccountLoggedInAsync(string userId) => HasActiveSessionAsync(userId);

    /// <summary>
    /// Alias para compatibilidade com interface existente.
    /// </summary>
    public Task<bool> IsSessionValidAsync(string userId) => HasActiveSessionAsync(userId);

    /// <summary>
    /// Estabelece uma nova sessão de jogo com dados inicializados.
    /// </summary>
    public async Task SetAccountSessionAsync(string userId, long accountId, TimeSpan? expiration = null)
    {
        var cacheKey = GetSessionKey(userId);
        var slidingExpiration = expiration ?? TimeSpan.FromMinutes(DEFAULT_SESSION_MINUTES);
        
        var sessionData = new GameSessionData
        {
            AccountId = accountId,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(slidingExpiration),
            Data = new Dictionary<string, object>()
        };

        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = slidingExpiration,
            PostEvictionCallbacks = 
            {
                new PostEvictionCallbackRegistration
                {
                    EvictionCallback = OnSessionEvicted
                }
            }
        };

        // Armazenar sessão no cache
        _cache.Set(cacheKey, sessionData, options);

        // Definir claims imutáveis (dados persistentes que não expiram com a sessão)
        await _identityService.AddClaimAsync(userId, Domain.Constants.Claims.AccountId, accountId.ToString());
        await _identityService.AddClaimAsync(userId, Domain.Constants.Claims.SessionStartTime, sessionData.CreatedAt.ToUnixTimeSeconds().ToString());

        // Métricas
        _sessionsCreated.Add(1, new KeyValuePair<string, object?>("user_id", userId));

        _logger.LogInformation("Game session established for user {UserId} with account {AccountId}, expires at {ExpiresAt}",
            userId, accountId, sessionData.ExpiresAt);
    }

    /// <summary>
    /// Renova a sessão existente atualizando o tempo de expiração.
    /// </summary>
    public Task RefreshSessionAsync(string userId)
    {
        var cacheKey = GetSessionKey(userId);

        if (_cache.TryGetValue(cacheKey, out var cachedData) && cachedData is GameSessionData sessionData)
        {
            // Atualizar tempo de expiração
            sessionData.ExpiresAt = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(DEFAULT_SESSION_MINUTES));

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(DEFAULT_SESSION_MINUTES),
                PostEvictionCallbacks = 
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = OnSessionEvicted
                    }
                }
            };

            _cache.Set(cacheKey, sessionData, options);

            // Métricas
            _sessionsRenewed.Add(1, new KeyValuePair<string, object?>("user_id", userId));

            _logger.LogDebug("Game session refreshed for user {UserId}, new expiration: {ExpiresAt}", 
                userId, sessionData.ExpiresAt);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Remove a sessão e todos os dados relacionados de forma síncrona e atômica.
    /// </summary>
    public async Task RevokeAccountSessionAsync(string userId)
    {
        var cacheKey = GetSessionKey(userId);

        // Remover do cache primeiro
        _cache.Remove(cacheKey);

        // Remoção síncrona e atômica dos claims de sessão
        try
        {
            await _identityService.RemoveClaimAsync(userId, Domain.Constants.Claims.SelectedCharacterId);
            // AccountId e SessionStartTime são mantidos como dados históricos/imutáveis
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing session claims for user {UserId}", userId);
        }

        // Métricas
        _sessionsRevoked.Add(1, new KeyValuePair<string, object?>("user_id", userId));

        _logger.LogInformation("Game session revoked for user {UserId}", userId);
    }

    /// <summary>
    /// Obtém o ID da conta ativa na sessão.
    /// </summary>
    public Task<long?> GetActiveAccountIdAsync(string userId)
    {
        var cacheKey = GetSessionKey(userId);

        if (_cache.TryGetValue(cacheKey, out var cachedData) && cachedData is GameSessionData sessionData)
        {
            return Task.FromResult<long?>(sessionData.AccountId);
        }

        return Task.FromResult<long?>(null);
    }

    /// <summary>
    /// Obtém o perfil da conta da sessão ativa.
    /// </summary>
    public async Task<AccountDto?> GetProfileAsync(string userId)
    {
        // Verificar se há sessão ativa primeiro
        if (!await HasActiveSessionAsync(userId))
        {
            return null;
        }

        try
        {
            // Usar o método existente que trabalha com o contexto do usuário atual
            return await _currentAccountService.GetDtoAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve profile for user {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Obtém dados da sessão de forma coerente.
    /// </summary>
    public Task<Dictionary<string, object>?> GetSessionDataAsync(string userId)
    {
        var cacheKey = GetSessionKey(userId);

        if (_cache.TryGetValue(cacheKey, out var cachedData) && cachedData is GameSessionData sessionData)
        {
            return Task.FromResult<Dictionary<string, object>?>(sessionData.Data);
        }

        return Task.FromResult<Dictionary<string, object>?>(null);
    }

    /// <summary>
    /// Atualiza dados da sessão de forma coerente.
    /// </summary>
    public Task UpdateSessionDataAsync(string userId, Dictionary<string, object> data)
    {
        var cacheKey = GetSessionKey(userId);

        if (_cache.TryGetValue(cacheKey, out var cachedData) && cachedData is GameSessionData sessionData)
        {
            sessionData.Data = data ?? new Dictionary<string, object>();
            
            // Recriar entrada no memória transitória para garantir que o callback de expiração seja mantido
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(DEFAULT_SESSION_MINUTES),
                PostEvictionCallbacks = 
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = OnSessionEvicted
                    }
                }
            };

            _cache.Set(cacheKey, sessionData, options);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retorna o TTL real baseado no campo ExpiresAt.
    /// </summary>
    public Task<TimeSpan?> GetSessionTtlAsync(string userId)
    {
        var cacheKey = GetSessionKey(userId);

        if (_cache.TryGetValue(cacheKey, out var cachedData) && cachedData is GameSessionData sessionData)
        {
            var remaining = sessionData.ExpiresAt - DateTimeOffset.UtcNow;
            return Task.FromResult<TimeSpan?>(remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero);
        }

        return Task.FromResult<TimeSpan?>(null);
    }

    // Character selection methods - usando claims como dados imutáveis

    /// <summary>
    /// Define o personagem selecionado como dado imutável.
    /// </summary>
    public async Task SetSelectedCharacterAsync(string userId, long characterId)
    {
        // Armazenar como claim imutável
        await _identityService.AddClaimAsync(userId, Domain.Constants.Claims.SelectedCharacterId, characterId.ToString());

        // Também armazenar nos dados da sessão para acesso rápido
        var sessionData = await GetSessionDataAsync(userId) ?? new Dictionary<string, object>();
        sessionData["SelectedCharacterId"] = characterId;
        await UpdateSessionDataAsync(userId, sessionData);

        _logger.LogInformation("Character {CharacterId} selected for user {UserId}", characterId, userId);
    }

    /// <summary>
    /// Obtém o personagem selecionado, priorizando dados da sessão.
    /// </summary>
    public async Task<long?> GetSelectedCharacterIdAsync(string userId)
    {
        // Tentar dados da sessão primeiro (mais rápido)
        var sessionData = await GetSessionDataAsync(userId);
        if (sessionData?.TryGetValue("SelectedCharacterId", out var characterId) == true)
        {
            return characterId is long id ? id : null;
        }

        // Fallback para claim (dado imutável)
        var claimValue = await _identityService.GetClaimValueAsync(userId, Domain.Constants.Claims.SelectedCharacterId);
        return claimValue != null && long.TryParse(claimValue, out var parsedId) ? parsedId : null;
    }

    /// <summary>
    /// Remove a seleção de personagem.
    /// </summary>
    public async Task ClearSelectedCharacterAsync(string userId)
    {
        await _identityService.RemoveClaimAsync(userId, Domain.Constants.Claims.SelectedCharacterId);

        // Remover dos dados da sessão também
        var sessionData = await GetSessionDataAsync(userId);
        if (sessionData?.ContainsKey("SelectedCharacterId") == true)
        {
            sessionData.Remove("SelectedCharacterId");
            await UpdateSessionDataAsync(userId, sessionData);
        }

        _logger.LogInformation("Character selection cleared for user {UserId}", userId);
    }

    // Private methods

    private static string GetSessionKey(string userId) => $"{SESSION_KEY_PREFIX}{userId}";

    /// <summary>
    /// Callback otimizado para expiração de sessão - delega processamento assíncrono.
    /// Evita bloqueios no thread de limpeza do cache.
    /// </summary>
    private void OnSessionEvicted(object key, object? value, EvictionReason reason, object? state)
    {
        if (reason == EvictionReason.Expired && 
            key.ToString()?.StartsWith(SESSION_KEY_PREFIX) == true &&
            value is GameSessionData sessionData)
        {
            var userId = sessionData.UserId;

            // Métricas imediatas (síncronas)
            _sessionsExpired.Add(1, new KeyValuePair<string, object?>("user_id", userId));

            _logger.LogInformation("Game session expired for user {UserId}", userId);

            // Delegação assíncrona para limpeza de claims - sem bloqueio!
            _expirationService.EnqueueExpiration(userId, "expired");
        }
    }

    /// <summary>
    /// Modelo de dados da sessão - centraliza todas as informações.
    /// </summary>
    private class GameSessionData
    {
        public required string UserId { get; set; }
        public required long AccountId { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public required DateTimeOffset ExpiresAt { get; set; }
        public required Dictionary<string, object> Data { get; set; }
    }
}
