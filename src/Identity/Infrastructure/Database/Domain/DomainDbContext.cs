using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Infrastructure.Identity.Entities;

namespace ServerGame.Infrastructure.Database.Domain;

public class DomainDbContext : IdentityDbContext<ApplicationUser>
{
    public DomainDbContext(DbContextOptions<DomainDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
