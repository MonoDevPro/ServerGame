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
    IAccountQueryService accountQueryService,
    ISessionManager sessionManager,
    IUser user,
    ILogger<LogoutAccountCommandHandler> logger
    ) : IRequestHandler<LogoutAccountCommand, Unit>
{
    public async Task<Unit> Handle(LogoutAccountCommand request, CancellationToken cancellationToken)
    {
        var accountId = await accountQueryService.GetIdAsync(user.Id!, cancellationToken);
        var account = await accountQueryService.GetByIdAsync(accountId, cancellationToken);
        
        account.Logout();

        await sessionManager.RevokeSessionAsync(user.Id!);

        logger.LogInformation("User logged out successfully.");
        
        return Unit.Value;
    }
}
