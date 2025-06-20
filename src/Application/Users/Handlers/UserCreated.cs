using System.ComponentModel.DataAnnotations;
using GameServer.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Users.Handlers;

public record UserCreatedNotification(
    [Required] string UserId
    ) : INotification;

public class UserCreatedHandler(ILogger<UserCreatedHandler> logger)
    : INotificationHandler<UserCreatedNotification>
{
    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Log the creation user event
        logger.LogInformation($"User {notification.UserId} created.");

        return Task.CompletedTask;
    }
}
