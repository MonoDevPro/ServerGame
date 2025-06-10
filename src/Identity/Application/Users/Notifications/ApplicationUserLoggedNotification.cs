using ServerGame.Application.Common.Interfaces;

namespace ServerGame.Application.Users.Notifications;

public record ApplicationUserLoggedNotification(
    string UserId,
    string UserName,
    string Email
) : INotification;
