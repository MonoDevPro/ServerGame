using Microsoft.EntityFrameworkCore;

namespace ServerGame.Infrastructure.Database.Common.Interceptors;

public abstract class SaveInterceptor : IPostSaveInterceptor, IPreSaveInterceptor
{
    public virtual Task PostSaveChangesAsync(DbContext context)
    {
        throw new NotImplementedException();
    }

    public virtual Task PreSaveChangesAsync(DbContext context)
    {
        throw new NotImplementedException();
    }
}
