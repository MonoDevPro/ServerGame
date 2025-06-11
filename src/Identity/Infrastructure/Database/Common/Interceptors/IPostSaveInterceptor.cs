using Microsoft.EntityFrameworkCore;

namespace ServerGame.Infrastructure.Database.Common.Interceptors;

public interface IPostSaveInterceptor
{
    /// <summary>
    /// Intercepta a ação de salvar após as alterações serem persistidas no banco de dados.
    /// </summary>
    /// <param name="context">O contexto do banco de dados.</param>
    Task PostSaveChangesAsync(DbContext context);
}
