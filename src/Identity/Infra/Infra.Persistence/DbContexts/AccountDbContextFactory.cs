using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServerGame.Infrastructure.Persistence.DbContexts;

public class AccountDbContextFactory : IDesignTimeDbContextFactory<AccountDbContext>
{
    public AccountDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AccountDbContext>()
            .UseNpgsql("Host=localhost;Port=45663;Username=postgres;Password=devpassword;Database=serverdb", npg =>
            {
                npg.MigrationsAssembly(typeof(AccountDbContext).Assembly.FullName);
            })
            .Options;

        return new AccountDbContext(options);
    }
}
