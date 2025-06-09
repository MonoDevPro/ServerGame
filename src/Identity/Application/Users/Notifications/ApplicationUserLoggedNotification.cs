using ServerGame.Application.Users.Notifications.Base;

namespace ServerGame.Application.Users.Notifications;

public record ApplicationUserLoggedNotification(
    string UserId,
    string UserName,
    string Email
) : ApplicationUserNotification;
