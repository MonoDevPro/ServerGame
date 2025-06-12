using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Commands.CreateAccount;
using ServerGame.Application.ApplicationUsers.Notifications;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.ApplicationUsers.NotificationHandlers;

public class ApplicationUserCreatedNotificationHandler(
    IMediator mediator,
    ILogger<ApplicationUserCreatedNotificationHandler> logger
    ) : INotificationHandler<ApplicationUserCreatedNotification>
{
    public async Task Handle(ApplicationUserCreatedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing ApplicationUserCreatedEvent for UserId {UserId} with Username {UserName} and Email {Email}",
                notification.UserId, 
                notification.UserName, 
                notification.Email);
            
            // Invocar o comando CreateAccountCommand para criar o Account
            var command = new CreateAccountCommand(
                Username.Create(notification.UserName),
                Email.Create(notification.Email)
            );

            await mediator.Send(command, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating Account for ApplicationUser {UserId}", notification.UserId);
            throw; // Re-throw to let the exception propagate
        }
        finally
        {
            logger.LogInformation(
                "Finished processing ApplicationUserCreatedEvent for UserId {UserId}", 
                notification.UserId);
        }
    }
}
