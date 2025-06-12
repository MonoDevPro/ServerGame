using ServerGame.Application.Common.Models;

namespace ServerGame.Application.ApplicationUsers.Services;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password);

    Task<Result> DeleteUserAsync(string userId);
}
