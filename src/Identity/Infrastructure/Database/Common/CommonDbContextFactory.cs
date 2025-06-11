using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServerGame.Infrastructure.Database.Common;

public class CommonDbContextFactory : IDesignTimeDbContextFactory<CommonDbContext>
{
    public CommonDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CommonDbContext>();

        // Use PostgreSQL for design-time
        optionsBuilder.UseNpgsql("Host=localhost;Database=serverdb;Username=postgres;Password=devpassword");

        return new CommonDbContext(optionsBuilder.Options);
    }
}
