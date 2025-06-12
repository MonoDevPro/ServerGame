using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerGame.Infrastructure.Database.Common.Interceptors.Interfaces;

namespace ServerGame.Infrastructure.Database.Common.Interceptors.Services;

public class DatabaseInterceptor(IServiceProvider provider, ILogger<DatabaseInterceptor> logger) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var interceptor = provider.GetServices<IPreSaveInterceptor>();
        
        foreach (var preSaveInterceptor in interceptor)
        {
            try
            {
                await preSaveInterceptor.PreSaveChangesAsync(eventData, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UnitOfWorkInterceptor: Error during PreSaveChangesAsync");
                throw;
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        
        var interceptor = provider.GetServices<IPostSaveInterceptor>();
        
        foreach (var postSaveInterceptor in interceptor)
        {
            try
            {
                await postSaveInterceptor.PostSaveChangesAsync(eventData, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UnitOfWorkInterceptor: Error during PostSaveChangesAsync");
                throw;
            }
        }
        
        return eventData.EntitiesSavedCount;
    }
}
