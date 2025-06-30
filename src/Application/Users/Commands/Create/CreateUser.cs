using GameServer.Application.Common.Models;
using GameServer.Application.Users.Services;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Users.Commands.Create;

public record CreateUserCommand(
    string Username,
    string Email,
    string Password
) : IRequest<(Result Result, string UserId)>;

public class CreateUserCommandHandler(
    IIdentityService identityService,
    ILogger<CreateUserCommandHandler> logger
    ) : IRequestHandler<CreateUserCommand, (Result Result, string UserId)>
{
    public async Task<(Result Result, string UserId)> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating user with username: {Username}, email: {Email}", 
            request.Username, request.Email);

        var (result, userId) = await identityService.CreateUserAsync(
            request.Username, 
            request.Email, 
            request.Password);

        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors);
            logger.LogWarning("Failed to create user {Username}: {Errors}", request.Username, errorMessage);
            
            return (result, string.Empty);
        }

        logger.LogInformation("User created successfully: {Username} (ID: {UserId})", 
            request.Username, userId);

        return (result, userId);
    }
}
