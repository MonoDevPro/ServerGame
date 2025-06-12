using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ServerGame.Infrastructure.Database.Application.Identity.Entities;

namespace ServerGame.Infrastructure.Database.Application.Identity;

public class ClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
{
    public ClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
    {
    }
    
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser applicationUser)
    {
        // pega as claims padrões (id, nome, roles, user-specific claims)
        var identity = await base.GenerateClaimsAsync(applicationUser);

        /*// adicione aqui suas claims custom
        identity.AddClaim(new Claim("MyCustomClaim", "MyValue"));

        // por exemplo, claim dinâmica
        if (user.IsVip)
            identity.AddClaim(new Claim("vip", "true"));*/

        return identity;
    }
}
