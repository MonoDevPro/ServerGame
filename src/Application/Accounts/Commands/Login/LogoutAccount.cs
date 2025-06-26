using System;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Security;
using GameServer.Application.Session;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Login;

[RequireGameSession]
public record LogoutAccountCommand : IRequest<Unit>;

public class LogoutAccountCommandHandler(
    ICurrentAccountService currentAccountService, 
    IUser user,
    IGameSessionService gameSessionService,
    ILogger<LogoutAccountCommandHandler> logger
    ) : IRequestHandler<LogoutAccountCommand, Unit>
{
    public async Task<Unit> Handle(LogoutAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await currentAccountService.GetForUpdateAsync(cancellationToken);
        account.Logout();

        await gameSessionService.RevokeAccountSessionAsync(user.Id!);

        logger.LogInformation("User logged out successfully.");
        
        return Unit.Value;
    }
}
