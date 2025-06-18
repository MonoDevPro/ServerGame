using System.Reflection;
using Infra.Identity.Persistence.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static ServerGame.Domain.Constants.Roles;

namespace Infra.Identity.Persistence.DbContexts;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.HasDefaultSchema("identity");
        
        // Apply configurations
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Add default data
        builder.Entity<ApplicationRole>().HasData(
            [
                new {Id = Guid.NewGuid().ToString(), Name = Player, NormalizedName = Player.ToUpperInvariant()},
                new {Id = Guid.NewGuid().ToString(), Name = Vip, NormalizedName = Vip.ToUpperInvariant()},
                new {Id = Guid.NewGuid().ToString(), Name = Moderator, NormalizedName = Moderator.ToUpperInvariant()},
                new {Id = Guid.NewGuid().ToString(), Name = GameMaster, NormalizedName = GameMaster.ToUpperInvariant()},
                new {Id = Guid.NewGuid().ToString(), Name = Administrator, NormalizedName = Administrator.ToUpperInvariant()},
                new {Id = Guid.NewGuid().ToString(), Name = Support, NormalizedName = Support.ToUpperInvariant()},
                new {Id = Guid.NewGuid().ToString(), Name = ContentCreator, NormalizedName = ContentCreator.ToUpperInvariant()}
                
            ]
        );
    }
}
