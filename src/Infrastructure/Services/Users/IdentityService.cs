using System.Security.Claims;
using GameServer.Application.Common.Interfaces.Identity;
using GameServer.Application.Common.Models;
using GameServer.Infrastructure.Services.Users.Identity.Extensions;
using Identity.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace GameServer.Infrastructure.Services.Users;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService)
    : IIdentityService
{
    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email
        };

        var result = await userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId);

        return user != null && await userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await userClaimsPrincipalFactory.CreateAsync(user);

        var result = await authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    private async Task<Result> DeleteUserAsync(ApplicationUser applicationUser)
    {
        var result = await userManager.DeleteAsync(applicationUser);

        return result.ToApplicationResult();
    }
    
    public async Task<bool> HasClaimAsync(string userId, string claimType)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var claims = await userManager.GetClaimsAsync(user);
        return claims.Any(c => c.Type == claimType);
    }

    public async Task<string?> GetClaimValueAsync(string userId, string claimType)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var claims = await userManager.GetClaimsAsync(user);
        return claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    public async Task<Result> AddClaimAsync(string userId, string claimType, string claimValue)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure([$"User not found: {userId}"]);

        // Remover claim existente se houver
        await RemoveClaimAsync(userId, claimType);

        var result = await userManager.AddClaimAsync(user, new Claim(claimType, claimValue));
        return result.ToApplicationResult();
    }

    public async Task<Result> RemoveClaimAsync(string userId, string claimType)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Result.Success();

        var claims = await userManager.GetClaimsAsync(user);
        var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType);
        
        if (claimToRemove != null)
        {
            var result = await userManager.RemoveClaimAsync(user, claimToRemove);
            return result.ToApplicationResult();
        }

        return Result.Success();
    }

    public async Task<Result> UpdateClaimAsync(string userId, string claimType, string newValue)
    {
        return await AddClaimAsync(userId, claimType, newValue);
    }

    public async Task<IList<Claim>> GetUserClaimsAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return new List<Claim>();

        return await userManager.GetClaimsAsync(user);
    }
}
