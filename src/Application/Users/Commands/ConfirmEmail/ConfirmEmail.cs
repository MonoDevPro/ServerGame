using GameServer.Application.Users.Services;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Users.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    string UserId,
    string Code,
    string? ChangedEmail
) : IRequest<ConfirmEmailResult>;

public record ConfirmEmailResult(
    bool Success,
    string? ErrorMessage
);

public class ConfirmEmailCommandHandler(
    IIdentityService identityService,
    ILogger<ConfirmEmailCommandHandler> logger
    ) : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
{
    public async Task<ConfirmEmailResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Confirming email for user: {UserId}", request.UserId);

        var result = await identityService.ConfirmEmailAsync(request.UserId, request.Code);

        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors);
            logger.LogWarning("Failed to confirm email for user {UserId}: {Errors}", request.UserId, errorMessage);
            
            return new ConfirmEmailResult(false, errorMessage);
        }

        logger.LogInformation("Email confirmed successfully for user: {UserId}", request.UserId);
        return new ConfirmEmailResult(true, null);
    }
}
