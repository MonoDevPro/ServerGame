using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ServerGame.Domain.Entities.Accounts;

namespace ServerGame.Infrastructure.Database.Domain;

public class DomainDbContextFactory : IDesignTimeDbContextFactory<DomainDbContext>
{
    public DomainDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DomainDbContext>()
            .UseNpgsql("Host=localhost;Port=37927;Username=postgres;Password=devpassword;Database=serverdb", npg =>
            {
                npg.MigrationsAssembly(typeof(DomainDbContext).Assembly.FullName);
            })
            .Options;

        return new DomainDbContext(options);
    }
}


public class DomainDbContext : DbContext
{
    public DomainDbContext(DbContextOptions<DomainDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        var thisAssembly = Assembly.GetExecutingAssembly();
        builder.ApplyConfigurationsFromAssembly(thisAssembly, type =>
            // só as classes que moram na pasta/namespace de domínio
            type.Namespace is not null
            && type.Namespace.StartsWith("ServerGame.Infrastructure.Database.Domain.Configurations")
        );
    }
}
