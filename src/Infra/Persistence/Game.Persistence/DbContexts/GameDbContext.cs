using System.Reflection;
using GameServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Persistence.DbContexts;

public class GameDbContext(DbContextOptions<GameDbContext> options) 
    : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.HasDefaultSchema("gameserver");

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
