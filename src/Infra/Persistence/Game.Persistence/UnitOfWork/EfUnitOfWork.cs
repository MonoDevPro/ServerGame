using Game.Persistence.DbContexts;
using GameServer.Application.Common.Interfaces.Persistence;

namespace Game.Persistence.UnitOfWork;

/// <summary>
/// Implementação de IUnitOfWork usando EF Core DbContext.
/// </summary>
public class EfUnitOfWork(GameDbContext context) : IUnitOfWork
{
    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // SaveChangesAsync retorna número de registros afetados
        var affected = await context.SaveChangesAsync(cancellationToken);
        return affected > 0;
    }
}
