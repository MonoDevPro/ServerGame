using GameServer.Application.Accounts.Commands.Create;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Domain.Entities;
using GameServer.Domain.Exceptions;
using GameServer.Domain.ValueObjects.Accounts;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Login;

public record LoginAccountCommand : IRequest<LoginAccountResult>;

public record LoginAccountResult(
    bool Success,
    string? AccountId,
    int ExpiresInSeconds,
    string? ErrorMessage
);

public class LoginAccountCommandHandler(
    IAccountService accountService,
    IGameSessionService sessionService,
    IUser user)
    : IRequestHandler<LoginAccountCommand, LoginAccountResult>
{
    public async Task<LoginAccountResult> Handle(LoginAccountCommand request, CancellationToken ct)
    {
        // 1) Recupere EM TRACKING a conta (nova ou existente)
        var account = await accountService.GetForUpdateAsync(ct);

        // 2) Faça o login
        account.Login(LoginInfo.Create(user.IpAddress!, DateTimeOffset.UtcNow));
            
        // 3) Persiste via UnitOfWorkBehavior (não chamamos SaveChanges aqui)
        //    mas precisamos da AccountId para sessão
        var accountId = account.Id.ToString();

        // 4) Cria/renova sessão de jogo
        await sessionService.SetAccountSessionAsync(
            user.Id!, accountId, expiration: null);

        // 5) Retorna DTO com accountId + TTL (em segundos)
        var ttl = await sessionService.GetSessionTtlAsync(user.Id!);
             
        return new LoginAccountResult(true, accountId, ttl?.Seconds ?? 0, null);
    }
}
