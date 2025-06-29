using System.Security.Claims;
using GameServer.Application.Common.Models;

namespace GameServer.Application.Users.Services;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    // Método atualizado para criação completa de usuário com email de confirmação
    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password);
    
    // Método para reenvio de email de confirmação
    Task<Result> ResendConfirmationEmailAsync(string email);
    
    // Método para confirmação de email
    Task<Result> ConfirmEmailAsync(string userId, string code);

    Task<Result> DeleteUserAsync(string userId);
    
    // Métodos para Claims
    Task<bool> HasClaimAsync(string userId, string claimType);
    Task<string?> GetClaimValueAsync(string userId, string claimType);
    Task<Result> AddClaimAsync(string userId, string claimType, string claimValue);
    Task<Result> RemoveClaimAsync(string userId, string claimType);
    Task<Result> UpdateClaimAsync(string userId, string claimType, string newValue);
    Task<IList<Claim>> GetUserClaimsAsync(string userId);
}
