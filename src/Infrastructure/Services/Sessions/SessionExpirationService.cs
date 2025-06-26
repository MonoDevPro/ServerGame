using System.Collections.Concurrent;
using GameServer.Application.Common.Interfaces.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameServer.Infrastructure.Services.Sessions;

/// <summary>
/// Serviço em background que processa expirações de sessão de forma assíncrona,
/// evitando bloqueios no thread de limpeza do cache.
/// </summary>
public class SessionExpirationService(
    IServiceProvider serviceProvider,
    ILogger<SessionExpirationService> logger
    ) : BackgroundService
{
    private readonly ConcurrentQueue<SessionExpirationItem> _expirationQueue = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Enfileira uma expiração de sessão para processamento assíncrono.
    /// </summary>
    public void EnqueueExpiration(string userId, string? reason = null)
    {
        _expirationQueue.Enqueue(new SessionExpirationItem(userId, reason, DateTimeOffset.UtcNow));
        
        logger.LogDebug("Session expiration enqueued for user {UserId}, reason: {Reason}", userId, reason ?? "expired");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Session expiration service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpirationsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Processa a cada 5 segundos
            }
            catch (OperationCanceledException)
            {
                // Esperado durante shutdown
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing session expirations");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Aguarda mais tempo em caso de erro
            }
        }

        logger.LogInformation("Session expiration service stopped");
    }

    private async Task ProcessExpirationsAsync(CancellationToken cancellationToken)
    {
        if (_expirationQueue.IsEmpty)
            return;

        await _semaphore.WaitAsync(cancellationToken);
        
        try
        {
            var processedCount = 0;
            var batch = new List<SessionExpirationItem>();

            // Processar em lotes de até 50 itens
            while (batch.Count < 50 && _expirationQueue.TryDequeue(out var item))
            {
                batch.Add(item);
            }

            if (batch.Count == 0)
                return;

            // Criar scope para usar serviços com lifetime scoped
            using var scope = serviceProvider.CreateScope();
            var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

            foreach (var item in batch)
            {
                try
                {
                    await ProcessSingleExpirationAsync(identityService, item, cancellationToken);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing expiration for user {UserId}", item.UserId);
                    
                    // Reencaminhar para retry se não for erro crítico
                    if (ShouldRetry(ex))
                    {
                        _expirationQueue.Enqueue(item with { RetryCount = item.RetryCount + 1 });
                    }
                }
            }

            if (processedCount > 0)
            {
                logger.LogDebug("Processed {ProcessedCount} session expirations", processedCount);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ProcessSingleExpirationAsync(IIdentityService identityService, SessionExpirationItem item, CancellationToken cancellationToken)
    {
        // Remover apenas claims de sessão (não dados históricos/imutáveis)
        await identityService.RemoveClaimAsync(item.UserId, Domain.Constants.Claims.SelectedCharacterId);
        
        logger.LogInformation("Session claims cleaned up for user {UserId} (expired {ExpirationTime}, reason: {Reason})", 
            item.UserId, item.ExpirationTime, item.Reason ?? "expired");
    }

    private static bool ShouldRetry(Exception ex)
    {
        // Retry para erros temporários, não para erros estruturais
        return ex is not ArgumentException && ex is not InvalidOperationException;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Session expiration service is stopping, processing remaining {QueueCount} items", _expirationQueue.Count);
        
        // Processar itens restantes antes de parar
        if (!_expirationQueue.IsEmpty)
        {
            try
            {
                await ProcessExpirationsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error processing remaining expirations during shutdown");
            }
        }

        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Item de expiração de sessão para processamento em background.
    /// </summary>
    private record SessionExpirationItem(string UserId, string? Reason, DateTimeOffset ExpirationTime, int RetryCount = 0);
}
