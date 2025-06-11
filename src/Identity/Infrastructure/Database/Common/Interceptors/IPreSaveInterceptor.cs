using Microsoft.EntityFrameworkCore;

namespace ServerGame.Infrastructure.Database.Common.Interceptors;

public interface IPreSaveInterceptor
{
    /// <summary>
    /// Intercepta a ação de salvar antes que as alterações sejam persistidas no banco de dados.
    /// </summary>
    /// <param name="context">O contexto do banco de dados.</param>
    Task PreSaveChangesAsync(DbContext context);
}
