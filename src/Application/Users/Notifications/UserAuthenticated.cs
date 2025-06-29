using GameServer.Application.Accounts.Commands.Create;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Users.Notifications;

public record UserAuthenticatedNotification : INotification;

public record UserLoggedOutNotification : INotification;

public class UserLoggedInHandler(ISender sender, ILogger<UserLoggedInHandler> logger)
    : INotificationHandler<UserAuthenticatedNotification>
{
    public async Task Handle(UserAuthenticatedNotification notification, CancellationToken cancellationToken)
    {
        // Log the user login event
        logger.LogInformation($"User authenticated event received.");

        // Here you can add additional logic, such as updating user status, logging, etc.
        // For example, you might want to update the last login time or send a welcome message.

        // Create account in domain or other services if needed
        await sender.Send(new CreateAccountCommand(), cancellationToken);
    }
}