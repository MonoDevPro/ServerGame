namespace ServerGame.Application.ApplicationUsers.Notifications;

// Application/Common/Notifications.cs
public record ApplicationUserCreatedNotification(
    string UserId,
    string UserName,
    string Email
    ) : INotification;
