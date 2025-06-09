using ServerGame.Application.Accounts.Notifications.Base;

namespace ServerGame.Application.Accounts.Notifications;

public record AccountCreatedNotification(
    long UserId,
    string UserName,
    string Email
) : AccountNotification;
