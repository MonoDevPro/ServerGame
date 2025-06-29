using System.Linq.Expressions;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Game.Persistence.Repositories.Reader;

public class ReaderRepository<TEntity>(DbContext context) 
    : IReaderRepository<TEntity>
    where TEntity : class
{
    private readonly IQueryable<TEntity> _context = context.Set<TEntity>();

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate), "Predicate cannot be null");

        var query = _context;
        
        if (ignoreQueryFilters)
            query = query.IgnoreQueryFilters();
        if (ignoreAutoIncludes)
            query = query.IgnoreAutoIncludes();
        
        // Aplica o predicado
        query = query.Where(predicate);

        // Checa existência
        return await query.AnyAsync(cancellationToken);
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
        where TResult : class
    {
        // 1. Definição do IQueryable base
        IQueryable<TEntity> query = trackingType switch
        {
            TrackingType.NoTracking => _context.AsNoTracking(),
            TrackingType.NoTrackingWithIdentityResolution => _context.AsNoTrackingWithIdentityResolution(),
            TrackingType.Tracking => _context.AsTracking(),
            _ => throw new ArgumentOutOfRangeException(nameof(trackingType), trackingType, null)
        };

        // 2. Includes
        if (include != null)
            query = include(query);

        // 3. Filtro
        if (predicate != null)
            query = query.Where(predicate);

        // 4. Projeção ou retorno da entidade
        if (selector != null)
            return await query
                .Select(selector)
                .SingleOrDefaultAsync(cancellationToken);
        else
            // Sem projeção, TEntity deve ser compatível com TResult
            // Aqui usamos Cast — se TResult for valor, pode lançar em tempo de execução
            return await query
                .Cast<TResult>()
                .SingleOrDefaultAsync(cancellationToken);
    }
    
    public async Task<TResult?> QuerySingleValueAsync<TResult>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        TrackingType trackingType = TrackingType.NoTracking,
        CancellationToken cancellationToken = default)
    {
        // 1. Definição do IQueryable base
        IQueryable<TEntity> query = trackingType switch
        {
            TrackingType.NoTracking => _context.AsNoTracking(),
            TrackingType.NoTrackingWithIdentityResolution => _context.AsNoTrackingWithIdentityResolution(),
            TrackingType.Tracking => _context.AsTracking(),
            _ => throw new ArgumentOutOfRangeException(nameof(trackingType), trackingType, null)
        };

        // 2. Includes
        if (include != null)
            query = include(query);

        // 3. Filtro
        if (predicate != null)
            query = query.Where(predicate);

        // 4. Projeção ou retorno da entidade
        if (selector != null)
        {
            return await query
                .Select(selector)
                .SingleOrDefaultAsync(cancellationToken);
        }
        else
        {
            // Sem projeção, TEntity deve ser compatível com TResult
            // Aqui usamos Cast — se TResult for valor, pode lançar em tempo de execução
            return await query
                .Cast<TResult>()
                .SingleOrDefaultAsync(cancellationToken);
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
        where TResult : class
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
