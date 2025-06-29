using System.Reflection;
using GameServer.Application.Characters.Services.Current;
using GameServer.Application.Common.Exceptions;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Security;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Common.Behaviours;

public class CharacterSessionBehavior<TRequest, TResponse>(
    ICurrentCharacterSelection selection,
    ICurrentCharacterSelector selector,
    IUser user,
    ILogger<CharacterSessionBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse> where TRequest : class, IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var attribute = request.GetType()
            .GetCustomAttribute<RequireCharacterAttribute>(inherit: true);

        if (attribute is null)
            return await next(cancellationToken);

        var hasCurrentCharacter = await selection.ExistsAsync(cancellationToken);
            
        // 2) valida seleção
        if (!await selection.ExistsAsync(cancellationToken))
            throw new CharacterRequiredException(
                "CHARACTER_NOT_SELECTED: No character is currently selected.");

        // 3) valida propriedade se não for permitido AllowNotOwner
        if (!attribute.AllowNotOwner)
        {
            var characterId = await selection.GetIdAsync(cancellationToken);
            if (!await selector.IsOwnerAsync(characterId, cancellationToken))
                throw new ForbiddenException(
                    "CHARACTER_NOT_OWNER: The selected character does not belong to the current user.");
        }
                
        logger.LogInformation("Character session check passed for user {UserId}, AllowNotOwner: {AllowNotOwner}",
            user.Id, attribute.AllowNotOwner);

        return await next(cancellationToken);
    }
}
