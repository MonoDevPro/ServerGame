using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Commands.CreateAccount;
using ServerGame.Application.Users.Notifications;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Users.NotificationHandlers;

public class ApplicationUserCreatedNotificationHandler : INotificationHandler<ApplicationUserCreatedNotification>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApplicationUserCreatedNotificationHandler> _logger;

    public ApplicationUserCreatedNotificationHandler(
        IMediator mediator,
        ILogger<ApplicationUserCreatedNotificationHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(ApplicationUserCreatedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing ApplicationUserCreatedEvent for UserId {UserId} with Username {UserName} and Email {Email}",
                notification.UserId, 
                notification.UserName, 
                notification.Email);
            
            // Invocar o comando CreateAccountCommand para criar o Account
            var command = new CreateAccountCommand(
                Username.Create(notification.UserName),
                Email.Create(notification.Email)
            );

            await _mediator.Send(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Account for ApplicationUser {UserId}", notification.UserId);
            throw; // Re-throw to let the exception propagate
        }
        finally
        {
            _logger.LogInformation(
                "Finished processing ApplicationUserCreatedEvent for UserId {UserId}", 
                notification.UserId);
        }
    }
}
