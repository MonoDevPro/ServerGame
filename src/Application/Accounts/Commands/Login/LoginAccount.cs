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
    public async Task Handle(LoginAccountCommand request, CancellationToken ct)
    {
        try
        {
            // 1) Se não existir, crie sem salvar ainda
            await sender.Send(new CreateAccountCommand(), ct);

            // 2) Recupere EM TRACKING a conta (nova ou existente)
            var account = await accountService.GetForUpdateAsync(ct);
            
            // 3) Faça o login
            account.Login(LoginInfo.Create(user.IpAddress!, DateTimeOffset.UtcNow));
            
            // 4) Retorne e deixe o UnitOfWorkBehavior salvar tudo de uma vez
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
