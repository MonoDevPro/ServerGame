using GameServer.Application.Accounts.Commands.Create;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Domain.Entities;
using GameServer.Domain.Exceptions;
using GameServer.Domain.ValueObjects.Accounts;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Login;

public record LoginAccountCommand : IRequest;

public class LoginAccountCommandHandler(
    IAccountService accountService,
    IUser user,
    ISender sender,
    ILogger<LoginAccountCommandHandler> logger)
    : IRequestHandler<LoginAccountCommand>
{
    public async Task Handle(LoginAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // OBS: Criar a conta apenas se não existir ao logar pela primeira vez.
            if (!await accountService.ExistsAsync(cancellationToken))
                await sender.Send(new CreateAccountCommand(), cancellationToken);
                
            var account = await accountService.GetAsync(cancellationToken);
            
            account.Login(LoginInfo.Create(user.IpAddress!, DateTimeOffset.Now));
            
            await accountService.UpdateAsync(account, cancellationToken);
            
        }
        catch (DomainException ex)
        {
            logger.LogError(ex, "Erro de domínio ao logar: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao logar");
        }
    }
}
