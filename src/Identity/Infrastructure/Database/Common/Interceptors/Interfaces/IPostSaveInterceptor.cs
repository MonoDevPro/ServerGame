using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ServerGame.Infrastructure.Database.Common.Interceptors.Interfaces;

public interface IPostSaveInterceptor
{
    /// <summary>
    /// Intercepta a ação de salvar após as alterações serem persistidas no banco de dados.
    /// </summary>
    /// <param name="contextData">O contexto do banco de dados.</param>
    /// <param name="cancellationToken"></param>
    Task PostSaveChangesAsync(SaveChangesCompletedEventData contextData, CancellationToken cancellationToken = default);
}
