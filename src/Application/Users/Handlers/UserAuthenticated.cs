using GameServer.Application.Common.Interfaces;
using GameServer.Domain.Events.Accounts;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Users.Handlers;

public record UserAuthenticatedNotification : INotification;

public class UserLoggedInHandler(IUser user, ILogger<UserLoggedInHandler> logger)
    : INotificationHandler<UserAuthenticatedNotification>
{
    public Task Handle(UserAuthenticatedNotification notification, CancellationToken cancellationToken)
    {
        // Log the user login event
        logger.LogInformation($"User {user} authenticated event received.");

        // Here you can add additional logic, such as updating user status, logging, etc.
        // For example, you might want to update the last login time or send a welcome message.

        return Task.CompletedTask;
    }
}
