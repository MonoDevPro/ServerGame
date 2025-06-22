using GameServer.Application.Accounts.Commands.Login;

namespace GameServer.Application.Users.Handlers;

public class UserLogoutHandler(ISender sender)
    : INotificationHandler<UserLoggedOutNotification>
{
    public async Task Handle(UserLoggedOutNotification notification, CancellationToken cancellationToken)
    {
        // - Enviar eventos para outros microserviços
        // - Registrar atividade de logout
        await sender.Send(new LogoutAccountCommand(), cancellationToken);
    }
}
