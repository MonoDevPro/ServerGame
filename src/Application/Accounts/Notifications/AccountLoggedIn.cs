using GameServer.Domain.Events.Accounts;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Notifications;

public class AccountLoggedInNotificationHandler(
    ILogger<AccountLoggedInNotificationHandler> logger
    ) : INotificationHandler<AccountLoggedIn>
{
    public Task Handle(AccountLoggedIn notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing AccountLoggedInEvent for AccountId {AccountId} with LoginInfo {LoginInfo}",
            notification.AccountId, notification.LoginInfo);
        
        return Task.CompletedTask;
    }
}
