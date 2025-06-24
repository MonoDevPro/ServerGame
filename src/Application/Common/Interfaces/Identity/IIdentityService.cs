
using System.Security.Claims;
using GameServer.Application.Common.Models;

namespace GameServer.Application.Common.Interfaces.Identity;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password);

    Task<Result> DeleteUserAsync(string userId);
    
    // Novos métodos para Claims
    Task<bool> HasClaimAsync(string userId, string claimType);
    Task<string?> GetClaimValueAsync(string userId, string claimType);
    Task<Result> AddClaimAsync(string userId, string claimType, string claimValue);
    Task<Result> RemoveClaimAsync(string userId, string claimType);
    Task<Result> UpdateClaimAsync(string userId, string claimType, string newValue);
    Task<IList<Claim>> GetUserClaimsAsync(string userId);
}
