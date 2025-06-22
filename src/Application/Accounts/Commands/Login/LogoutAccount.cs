using System;
using GameServer.Application.Accounts.Services;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Login;

public record LogoutAccountCommand : IRequest;

public class LogoutAccountCommandHandler(IAccountService accountService, ILogger<LogoutAccountCommandHandler> logger) : IRequestHandler<LogoutAccountCommand>
{
    private readonly IAccountService _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
    private readonly ILogger<LogoutAccountCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Handle(LogoutAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountService.GetForUpdateAsync(cancellationToken);

            account.Logout();

            _logger.LogInformation("User logged out successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout process");
            throw;
        }
    }
}