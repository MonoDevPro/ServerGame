using System;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Login;

public record LogoutAccountCommand : IRequest;

public class LogoutAccountCommandHandler(
    IAccountService accountService, 
    IUser user,
    IGameSessionService gameSessionService,
    ILogger<LogoutAccountCommandHandler> logger
    ) : IRequestHandler<LogoutAccountCommand>
{
    public async Task Handle(LogoutAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await accountService.GetForUpdateAsync(cancellationToken);
            account.Logout();

            await gameSessionService.RevokeAccountSessionAsync(user.Id!);

            logger.LogInformation("User logged out successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout process");
            throw;
        }
    }
}
