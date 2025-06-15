using Microsoft.AspNetCore.Identity;

namespace Infra.Identity.Persistence.Entities;

public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; private set; } = true;
}
