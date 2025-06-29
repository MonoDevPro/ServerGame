using GameServer.Application.Common.Models;
using GameServer.Application.Users.Services;
using GameServer.Domain.Rules;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Users.Commands.Create;

public record CreateUserCommand(
    string Username,
    string Email,
    string Password
) : IRequest<CreateUserResult>;

public record CreateUserResult(
    bool Success,
    string? UserId,
    string? ErrorMessage
);

public class CreateUserCommandHandler(
    IIdentityService identityService,
    ILogger<CreateUserCommandHandler> logger
    ) : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
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
            
            return new CreateUserResult(false, null, errorMessage);
        }

        logger.LogInformation("User created successfully: {Username} (ID: {UserId})", 
            request.Username, userId);

        return new CreateUserResult(true, userId, null);
    }
}
