using ServerGame.Application.Common.Interfaces;

namespace ServerGame.Application.Users.Notifications;

// Application/Common/Notifications.cs
public record ApplicationUserCreatedNotification(
    string UserId,
    string UserName,
    string Email
    ) : INotification;
