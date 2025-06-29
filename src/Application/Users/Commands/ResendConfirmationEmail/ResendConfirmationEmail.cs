using GameServer.Application.Users.Services;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Users.Commands.ResendConfirmationEmail;

public record ResendConfirmationEmailCommand(
    string Email
) : IRequest<Unit>;

public class ResendConfirmationEmailCommandHandler(
    IIdentityService identityService,
    ILogger<ResendConfirmationEmailCommandHandler> logger
    ) : IRequestHandler<ResendConfirmationEmailCommand, Unit>
{
    public async Task<Unit> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Resending confirmation email to: {Email}", request.Email);

        await identityService.ResendConfirmationEmailAsync(request.Email);

        logger.LogInformation("Confirmation email resent to: {Email}", request.Email);
        return Unit.Value;
    }
}
