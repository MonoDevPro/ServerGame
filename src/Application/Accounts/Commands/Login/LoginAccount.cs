using GameServer.Application.Accounts.Commands.Create;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Security;
using GameServer.Application.Session;
using GameServer.Domain.Entities;
using GameServer.Domain.Exceptions;
using GameServer.Domain.ValueObjects.Accounts;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Login;

public record LoginAccountCommand : IRequest<LoginAccountResult>;

public record LoginAccountResult(
    bool Success,
    long? AccountId,
    int ExpiresInSeconds,
    string? ErrorMessage
);

public class LoginAccountCommandHandler(
    IAccountQueryService query,
    ISessionManager sessionManager,
    IUser user)
    : IRequestHandler<LoginAccountCommand, LoginAccountResult>
{
    public async Task<LoginAccountResult> Handle(LoginAccountCommand request, CancellationToken ct)
    {
        // 1) Recupere EM TRACKING a conta existente
        var accountId = await query.GetIdAsync(user.Id!, ct);
        var account = await query.GetByIdAsync(accountId, ct);
        
        // 2) Faça o login
        account.Login(LoginInfo.Create(user.IpAddress!, DateTimeOffset.UtcNow));
            
        // 3) Persiste via UnitOfWorkBehavior (não chamamos SaveChanges aqui)
        //    mas precisamos da AccountId para sessão
        
        // 4) Cria/renova sessão de jogo
        await sessionManager.SetSessionAsync(
            user.Id!, accountId, expiration: null);

        // 5) Retorna DTO com accountId + TTL (em segundos)
        var ttl = await sessionManager.GetSessionTtlAsync(user.Id!);
             
        return new LoginAccountResult(true, accountId, ttl?.Seconds ?? 0, null);
    }
}
