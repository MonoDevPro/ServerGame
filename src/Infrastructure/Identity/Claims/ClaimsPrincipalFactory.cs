using System.Security.Claims;
using Identity.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GameServer.Infrastructure.Identity.Claims;

public class ClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ApplicationUser>(userManager, optionsAccessor)
{
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
