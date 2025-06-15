using System.Security.Claims;
using ServerGame.Application.Common.Interfaces;

namespace ServerGame.Api.Services;

public class CurrentUser(IHttpContextAccessor ctx) : IUser
{
    // UserId (sub ou nameidentifier)
    public string? Id    => ctx.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
