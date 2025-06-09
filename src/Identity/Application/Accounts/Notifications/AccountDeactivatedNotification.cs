using ServerGame.Application.Accounts.Notifications.Base;

namespace ServerGame.Application.Accounts.Notifications;

public record AccountDeactivatedNotification(
    string UserId,
    string UserName,
    string Email
) : AccountNotification;
