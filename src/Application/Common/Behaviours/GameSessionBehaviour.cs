using System.Reflection;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Accounts.Services.Current;
using GameServer.Application.Common.Exceptions;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Security;
using GameServer.Application.Session;
using GameServer.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Common.Behaviours;

public class GameSessionBehaviour<TRequest, TResponse>(
    ISessionManager sessionManager,
    ICurrentAccountService currentAccountService,
    IUser user,
    ILogger<GameSessionBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var attribute = request.GetType()
                .GetCustomAttribute<RequireGameSessionAttribute>(inherit: true);

            if (attribute is null)
                return await next(cancellationToken);

            // Usuário autenticado?
            var userId = user.Id 
                ?? throw new UnauthorizedAccessException("User not authenticated");

            // Conta de jogo existe?
            if (!await currentAccountService.ExistsAsync(cancellationToken))
                throw new GameSessionRequiredException("GAME_SESSION_REQUIRED: You must login to your game account first");

            // Sessão ativa?
            var hasSession = await sessionManager.HasActiveSessionAsync(userId);
            if (!hasSession && !attribute.AllowExpiredSession)
                throw new GameSessionRequiredException("GAME_SESSION_REQUIRED: You must login to your game account first");

            // Renovar sessão apenas se existir
            if (hasSession)
            {
                await sessionManager.RefreshSessionAsync(userId);
                logger.LogDebug("Game session refreshed for user {UserId}", userId);
            }

            // Verificar nível mínimo
            if (attribute.MinAccountType > AccountType.Player)
            {
                var profile = await currentAccountService.GetDtoAsync(cancellationToken);
                if (profile.AccountType < attribute.MinAccountType)
                    throw new ForbiddenException(
                        $"Insufficient level. Required: {attribute.MinAccountType}, Current: {profile.AccountType}");
            }

            logger.LogInformation(
                "Game session check passed for user {UserId}, AllowExpiredSession={AllowExpired}, MinLevel={MinLevel}",
                userId, attribute.AllowExpiredSession, attribute.MinAccountType);

            return await next(cancellationToken);
        }
    }
