using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Persistence.DbContexts;

public class IdentityDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Port=43135;Username=postgres;Password=devpassword;Database=serverdb", npg =>
            {
                npg.MigrationsAssembly(Assembly.GetExecutingAssembly());
            })
            .Options;

        return new ApplicationDbContext(options);
    }
}
