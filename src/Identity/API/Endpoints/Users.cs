using Infra.Identity.Persistence.Entities;
using ServerGame.Api.Infrastructure;

namespace ServerGame.Api.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapIdentityApi<ApplicationUser>();
    }
}
