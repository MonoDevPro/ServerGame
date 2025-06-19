using GameServer.Application.Common.Interfaces.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameServer.Infrastructure.Persistence.Repositories.Writer;

public class WriterRepository<TEntity>(DbContext dbContext, ILogger logger) 
    : IWriterRepository<TEntity>
    where TEntity : class
{
    private readonly DbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();

    public virtual async Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao adicionar entidade no banco de dados: {Message}", ex.Message);
            throw;
        }
    }

    public virtual Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar entidade no banco de dados: {Message}", ex.Message);
            throw;
        }
    }

    public virtual Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao excluir entidade do banco de dados: {Message}", ex.Message);
            throw;
        }
    }

    public Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao excluir entidades do banco de dados: {Message}", ex.Message);
            throw;
        }
    }

    public virtual async Task<bool> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao salvar alterações no banco de dados: {Message}", ex.Message);
            throw;
        }
    }
}
