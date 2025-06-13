using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ServerGame.Infrastructure.Database.Common.Interceptors.Interfaces;

public interface IPreSaveInterceptor<TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Intercepta a ação de salvar antes que as alterações sejam persistidas no banco de dados.
    /// </summary>
    /// <param name="contextData">O contexto do banco de dados.</param>
    /// <param name="cancellationToken"></param>
    Task PreSaveChangesAsync(DbContextEventData contextData, CancellationToken cancellationToken = default);
}
