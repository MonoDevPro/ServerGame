namespace ServerGame.Application.ApplicationUsers.Notifications;

public record ApplicationUserLoggedNotification(
    string UserId,
    string UserName,
    string Email
) : INotification;
