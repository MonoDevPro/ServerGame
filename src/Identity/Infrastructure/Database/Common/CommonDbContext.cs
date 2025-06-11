using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Infrastructure.Identity.Entities;

namespace ServerGame.Infrastructure.Database.Common;

public class CommonDbContext : DbContext
{
    public CommonDbContext(DbContextOptions<CommonDbContext> options)
        : base(options)
    {
    }
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<Account> Accounts => Set<Account>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);

    public override EntityEntry Entry(object entity) => base.Entry(entity);
}
