using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using ServerGame.Application.Common.Interfaces.Data;

namespace ServerGame.Application.Common.Interfaces.Persistence.Repository;

public interface IReaderRepository<TEntity>
    where TEntity : class
{
    // Métodos otimizados para contagem e verificação de existência
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default);
    Task<long> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null, 
        CancellationToken cancellationToken = default);
    
    // Adicionar métodos com projeção para outros tipos
    Task<List<TResult>> QueryListAsync<TResult>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        TrackingType trackingType = TrackingType.NoTracking,
        CancellationToken cancellationToken = default);

    Task<TResult?> QuerySingleAsync<TResult>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        TrackingType trackingType = TrackingType.NoTracking,
        CancellationToken cancellationToken = default);
    
    // Novos métodos para paginação
    Task<IPagedList<TResult>> QueryPagedListAsync<TResult>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        int pageIndex = 0,
        int pageSize = 10,
        TrackingType trackingType = TrackingType.NoTracking,
        CancellationToken cancellationToken = default,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false);
}
