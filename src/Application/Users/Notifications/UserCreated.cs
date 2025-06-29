using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Users.Notifications;

public record UserCreatedNotification(
    [Required] string UserId
    ) : INotification;

public class UserCreatedHandler(ILogger<UserCreatedHandler> logger)
    : INotificationHandler<UserCreatedNotification>
{
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Log the creation user event
        logger.LogInformation($"User {notification.UserId} created.");

        // Não criamos a conta aqui, apenas após o usuario se autenticar
        // Isso é feito no handler de autenticação do usuário
        // await sender.Send(new CreateAccountCommand(), cancellationToken);

        await Task.CompletedTask;
    }
}
