
using GameServer.Application.Common.Models;

namespace GameServer.Application.Common.Interfaces.Identity;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password);

    Task<Result> DeleteUserAsync(string userId);
}
