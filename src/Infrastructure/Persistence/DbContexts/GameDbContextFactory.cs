using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GameServer.Infrastructure.Persistence.DbContexts;

public class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
{
    public GameDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseNpgsql("Host=localhost;Port=43135;Username=postgres;Password=devpassword;Database=serverdb", npg =>
            {
                npg.MigrationsAssembly(Assembly.GetExecutingAssembly());
            })
            .Options;

        return new GameDbContext(options);
    }
}
