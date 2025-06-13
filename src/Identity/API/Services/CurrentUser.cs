using System.Security.Claims;
using ServerGame.Application.Common.Interfaces;

namespace ServerGame.Api.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _ctx;
    public CurrentUser(IHttpContextAccessor ctx) => _ctx = ctx;

    // UserId (sub ou nameidentifier)
    public string? Id    => _ctx.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Username (tipicamente armazenado na ClaimTypes.Name)
    /*public string? Name  => _ctx.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
                            ?? _ctx.HttpContext?.User?.Identity?.Name;

    // Email (usando a claim ClaimTypes.Email)
    public string? Email => _ctx.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);*/
}
