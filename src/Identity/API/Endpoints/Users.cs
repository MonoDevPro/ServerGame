using ServerGame.Api.Infrastructure;
using ServerGame.Infrastructure.Identity;

namespace ServerGame.Api.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapIdentityApi<ApplicationUser>();
    }
}
