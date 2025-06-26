using System.Reflection;
using GameServer.Application.Common.Exceptions;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Security;
using GameServer.Application.Session;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Common.Behaviours;

public class GameSessionBehavior<TRequest, TResponse>(
    IGameSessionService gameSessionService,
    IUser user,
    ILogger<GameSessionBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse> where TRequest : class, IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var gameSessionAttributes = request.GetType().GetCustomAttributes<RequireGameSessionAttribute>().ToList();

        // Verificação de Game Session
        if (gameSessionAttributes.Count > 0)
        {
            if (user.Id == null)
                throw new UnauthorizedAccessException("User not authenticated");

            var attribute = gameSessionAttributes.First();

            // Verificar se sessão é válida ou permitir sessão expirada
            bool sessionValid = await gameSessionService.IsSessionValidAsync(user.Id);
            bool sessionExists = await gameSessionService.IsAccountLoggedInAsync(user.Id);

            if (!sessionExists)
                throw new GameSessionRequiredException("GAME_SESSION_REQUIRED: No session found");

            if (!sessionValid && !attribute.AllowExpiredSession)
                throw new GameSessionRequiredException("GAME_SESSION_REQUIRED: You must login to your game account first");

            // Verificar nível mínimo se especificado
            if (attribute.MinAccountType > 0)
            {
                var profile = await gameSessionService.GetProfileAsync(user.Id);
                if (profile == null)
                    throw new ForbiddenException("Unable to retrieve user profile for level verification");

                // Usar AccountType como proxy para level (Player=0, VIP=1, GameMaster=2, Staff=3, Administrator=4)
                var userLevel = profile.AccountType;
                if (userLevel < attribute.MinAccountType)
                    throw new ForbiddenException($"Insufficient level. Required: {attribute.MinAccountType}, Current: {userLevel} ({profile.AccountType})");
            }

            // Renovar sessão se válida (não para sessões expiradas)
            if (sessionValid)
            {
                await gameSessionService.RefreshSessionAsync(user.Id);
                logger.LogDebug("Game session refreshed for user {UserId}", user.Id);
            }

            logger.LogInformation("Game session check passed for user {UserId}, AllowExpiredSession: {AllowExpired}, MinLevel: {MinLevel}",
                user.Id, attribute.AllowExpiredSession, attribute.MinAccountType);
        }

        return await next(cancellationToken);
    }
}
