using System.Reflection;
using Identity.Persistence.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static GameServer.Domain.Constants.Roles;

namespace Identity.Persistence.DbContexts;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.HasDefaultSchema("identity");
        
        // Apply configurations
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // 1) IDs pré‑gerados (você gera uma única vez, copia e cola aqui)
        const string PlayerId        = "f47ac10b-58cc-4372-a567-0e02b2c3d479";
        const string VipId           = "9c858901-8a57-4791-81fe-4c455b099bc9";
        const string ModeratorId     = "3d6fa134-5a4f-4d32-bb3e-9f4f6c290f8b";
        const string GameMasterId    = "2f1ad5c7-6c30-4c6e-8b9d-cd34fe8fb1a1";
        const string AdministratorId = "7e57d004-2b97-0e7a-b45f-5387367791cd";
        const string SupportId       = "6c1f4b71-4855-4f15-8c15-1f4df61a1e1f";
        const string ContentCreatorId= "5a9c1b2d-ee7d-4a8b-a2f7-dc1f6e5b3a2c";

        // 2) Monta o array de seed usando só valores estáticos
        var seedRoles = new[]
        {
            new ApplicationRole { Id = PlayerId,        Name = Player,        NormalizedName = Player.ToUpperInvariant() },
            new ApplicationRole { Id = VipId,           Name = Vip,           NormalizedName = Vip.ToUpperInvariant() },
            new ApplicationRole { Id = ModeratorId,     Name = Moderator,     NormalizedName = Moderator.ToUpperInvariant() },
            new ApplicationRole { Id = GameMasterId,    Name = GameMaster,    NormalizedName = GameMaster.ToUpperInvariant() },
            new ApplicationRole { Id = AdministratorId, Name = Administrator, NormalizedName = Administrator.ToUpperInvariant() },
            new ApplicationRole { Id = SupportId,       Name = Support,       NormalizedName = Support.ToUpperInvariant() },
            new ApplicationRole { Id = ContentCreatorId,Name = ContentCreator,NormalizedName = ContentCreator.ToUpperInvariant() }
        };

        // 3) Aplica o seed
        builder.Entity<ApplicationRole>().HasData(seedRoles);
    }
}
