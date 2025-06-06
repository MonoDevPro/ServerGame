using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Database.Repository;

namespace ServerGame.Infrastructure.Data.Repositories.Writer;

internal class WriterRepository<TEntity> : IWriterRepository<TEntity>
    where TEntity : class
{
    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;
    private readonly ILogger<WriterRepository<TEntity>> _logger;

    public WriterRepository(DbContext dbContext, ILogger<WriterRepository<TEntity>> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = dbContext.Set<TEntity>();
        _logger = logger;
    }

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
            _logger.LogError(ex, "Erro ao adicionar entidade no banco de dados: {Message}", ex.Message);
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
            _logger.LogError(ex, "Erro ao atualizar entidade no banco de dados: {Message}", ex.Message);
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
            _logger.LogError(ex, "Erro ao excluir entidade do banco de dados: {Message}", ex.Message);
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
            _logger.LogError(ex, "Erro ao excluir entidades do banco de dados: {Message}", ex.Message);
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
            _logger.LogError(ex, "Erro ao salvar alterações no banco de dados: {Message}", ex.Message);
            throw;
        }
    }
}
