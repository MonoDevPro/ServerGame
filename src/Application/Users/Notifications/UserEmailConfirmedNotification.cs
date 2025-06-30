namespace GameServer.Application.Users.Notifications;

/// <summary>
/// Notificação disparada quando um usuário confirma seu email
/// </summary>
public record UserEmailConfirmedNotification(string UserId) : INotification;
