using System.Reflection;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Exceptions;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Security;
using GameServer.Application.Session;
using GameServer.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Common.Behaviours;

public class SessionBehaviour<TRequest, TResponse>(
    ISessionManager sessionManager,
    ICurrentAccountService currentAccountService,
    IUser user,
    ILogger<SessionBehaviour<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse> where TRequest : class, IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var gameSessionAttributes = request.GetType().GetCustomAttributes<RequireGameSessionAttribute>().ToList();

        // Verificação de Game Session
        if (gameSessionAttributes.Count > 0)
        {
            var attribute = gameSessionAttributes.First();
            
            if (!await currentAccountService.ExistsAsync(cancellationToken))
                throw new GameSessionRequiredException("GAME_SESSION_REQUIRED: You must login to your game account first");

            var sessionExists = await sessionManager.HasActiveSessionAsync(user.Id!);
            if (!sessionExists && !attribute.AllowExpiredSession)
                throw new GameSessionRequiredException("GAME_SESSION_REQUIRED: You must login to your game account first");
            
            // Renovar sessão
            await sessionManager.RefreshSessionAsync(user.Id!);
            logger.LogDebug("Game session refreshed for user {UserId}", user.Id);
            
            // Verificar nível mínimo se especificado
            if (attribute.MinAccountType > AccountType.Player)
            {
                var profile = await currentAccountService.GetDtoAsync(cancellationToken);

                // Usar AccountType como proxy para level (Player=0, VIP=1, GameMaster=2, Staff=3, Administrator=4)
                var userLevel = profile.AccountType;
                if (userLevel < attribute.MinAccountType)
                    throw new ForbiddenException($"Insufficient level. Required: {attribute.MinAccountType}, Current: {userLevel} ({profile.AccountType})");
            }

            logger.LogInformation("Game session check passed for user {UserId}, AllowExpiredSession: {AllowExpired}, MinLevel: {MinLevel}",
                user.Id, attribute.AllowExpiredSession, attribute.MinAccountType);
        }

        return await next(cancellationToken);
    }
}
