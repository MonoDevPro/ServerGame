using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ServerGame.Infrastructure.Database.Application.Identity.Entities;

namespace ServerGame.Infrastructure.Database.Application;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Port=37927;Username=postgres;Password=devpassword;Database=serverdb", npg =>
            {
                npg.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            })
            .Options;

        return new ApplicationDbContext(options);
    }
}

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options
    ) : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var thisAssembly = Assembly.GetExecutingAssembly();
        builder.ApplyConfigurationsFromAssembly(thisAssembly, type =>
            // só as classes que moram na pasta/namespace de domínio
            type.Namespace is not null
            && type.Namespace.StartsWith("ServerGame.Infrastructure.Database.Application.Configurations")
        );
    }
}
