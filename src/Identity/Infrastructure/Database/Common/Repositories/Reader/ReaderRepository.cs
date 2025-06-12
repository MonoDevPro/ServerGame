using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Models;

namespace ServerGame.Infrastructure.Database.Common.Repositories.Reader;

internal class ReaderRepository<TEntity> : IReaderRepository<TEntity>
    where TEntity : class
{
    private readonly IQueryable<TEntity> _context;
    public ReaderRepository(DbContext context) => _context = context.Set<TEntity>();

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.AnyAsync(predicate, cancellationToken);
    }

    public async Task<long> CountAsync(
        Expression<Func<TEntity, bool>>? predicate, 
        CancellationToken cancellationToken = default)
    {
        // Se o filtro for nulo, conta todos os registros
        if (predicate is null)
            return await _context.LongCountAsync(cancellationToken);
        // Caso contrário, aplica o filtro e conta os registros correspondentes
        else
            return await _context.CountAsync(predicate, cancellationToken);
    }

    public async Task<List<TResult>> QueryListAsync<TResult>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        TrackingType trackingType = TrackingType.NoTracking,
        CancellationToken cancellationToken = default)
    {
        // 1. Definição do IQueryable base conforme tipo de rastreamento
        IQueryable<TEntity> query = trackingType switch
        {
            TrackingType.NoTracking => _context.AsNoTracking(),
            TrackingType.NoTrackingWithIdentityResolution => _context.AsNoTrackingWithIdentityResolution(),
            TrackingType.Tracking => _context.AsTracking(),
            _ => throw new ArgumentOutOfRangeException(nameof(trackingType), trackingType, null)
        };

        if (include is not null)
            query = include(query);

        if (predicate is not null)
            query = query.Where(predicate);

        // 6. Projeção (Select) e paginação
        if (selector is not null)
        {
            var projected = query.Select(selector);
            return await projected.ToListAsync(cancellationToken);
        }
        else
        {
            // Sem projeção, retorna entidade diretamente
            return await query.Cast<TResult>().ToListAsync(cancellationToken);
        }
    }

    public async Task<TResult?> QuerySingleAsync<TResult>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        TrackingType trackingType = TrackingType.NoTracking,
        CancellationToken cancellationToken = default)
    {
        // 1. Definição do IQueryable base conforme tipo de rastreamento
        IQueryable<TEntity> query = trackingType switch
        {
            TrackingType.NoTracking => _context.AsNoTracking(),
            TrackingType.NoTrackingWithIdentityResolution => _context.AsNoTrackingWithIdentityResolution(),
            TrackingType.Tracking => _context.AsTracking(),
            _ => throw new ArgumentOutOfRangeException(nameof(trackingType), trackingType, null)
        };

        // 2. Include de navegações
        if (include is not null)
            query = include(query);

        // 3. Filtro Where
        if (predicate is not null)
            query = query.Where(predicate);

        // 6. Projeção (Select) e paginação
        if (selector is not null)
        {
            var projected = query.Select(selector);
            return await projected.SingleOrDefaultAsync(cancellationToken);
        }
        else
        {
            // Sem projeção, retorna entidade diretamente
            return await query.Cast<TResult>().SingleOrDefaultAsync(cancellationToken);
        }
    }

    public async Task<IPagedList<TResult>> QueryPagedListAsync<TResult>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        int pageNumber = 1,
        int pageSize = 20,
        TrackingType trackingType = TrackingType.NoTracking,
        CancellationToken cancellationToken = default,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        IQueryable<TEntity> query = trackingType switch
        {
            TrackingType.NoTracking => _context.AsNoTracking(),
            TrackingType.NoTrackingWithIdentityResolution => _context.AsNoTrackingWithIdentityResolution(),
            TrackingType.Tracking => _context.AsTracking(),
            _ => throw new ArgumentOutOfRangeException(nameof(trackingType), trackingType, null)
        };

        if (ignoreQueryFilters)
            query = query.IgnoreQueryFilters();
        if (ignoreAutoIncludes)
            query = query.IgnoreAutoIncludes();

        if (include is not null)
            query = include(query);

        if (predicate is not null)
            query = query.Where(predicate);

        if (orderBy is not null)
            query = orderBy(query);

        if (selector is not null)
        {
            var projected = query.Select(selector);
            return await PaginatedList<TResult>.CreateAsync(
                projected,
                pageNumber,
                pageSize,
                cancellationToken);
        }
        else
        {
            // no selector: cast entity to TResult
            IQueryable<TResult> casted = query.Cast<TResult>();
            return await PaginatedList<TResult>.CreateAsync(
                casted,
                pageNumber,
                pageSize,
                cancellationToken);
        }
    }
}
