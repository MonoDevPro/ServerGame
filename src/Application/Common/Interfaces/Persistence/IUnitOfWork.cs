namespace GameServer.Application.Common.Interfaces.Persistence;

/// <summary>
/// Interface que define a unidade de trabalho (Unit of Work).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persiste todas as alterações pendentes no contexto.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>True se pelo menos uma linha foi afetada.</returns>
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}
