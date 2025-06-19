using Microsoft.AspNetCore.Identity;

namespace GameServer.Infrastructure.Identity.Persistence.Entities;

public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; private set; } = true;
}
