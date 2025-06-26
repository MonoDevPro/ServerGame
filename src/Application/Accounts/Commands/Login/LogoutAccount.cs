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
    IAccountService accountService,
    IUser user,
    ISessionManager sessionManager,
    ILogger<LogoutAccountCommandHandler> logger
    ) : IRequestHandler<LogoutAccountCommand, Unit>
{
    public async Task<Unit> Handle(LogoutAccountCommand request, CancellationToken cancellationToken)
    {
        var accountId = await currentAccountService.GetIdAsync(cancellationToken);
        var account = await accountService.GetForUpdateAsync(accountId, cancellationToken);
        
        account.Logout();

        await sessionManager.RevokeSessionAsync(user.Id!);

        logger.LogInformation("User logged out successfully.");
        
        return Unit.Value;
    }
}
