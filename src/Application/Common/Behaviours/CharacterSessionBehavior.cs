using System.Reflection;
using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Exceptions;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Security;
using GameServer.Application.Session;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Common.Behaviours;

public class CharacterSessionBehavior<TRequest, TResponse>(
    ICurrentCharacterService currentCharacterService,
    IUser user,
    ILogger<CharacterSessionBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse> where TRequest : class, IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var characterAttributes = request.GetType().GetCustomAttributes<RequireCharacterAttribute>().ToList();

        if (characterAttributes.Count > 0)
        {
            if (user.Id == null)
                throw new UnauthorizedAccessException("User not authenticated");

            var attribute = characterAttributes.First();

            // Obter personagem selecionado da sessão
            var hasCurrentCharacter = await currentCharacterService.HasCurrentCharacterAsync(cancellationToken);

            // Verificar se é obrigatório ter personagem selecionado
            if (!hasCurrentCharacter && !attribute.AllowNoCharacterSelected)
                throw new CharacterRequiredException("CHARACTER_REQUIRED: No character selected. Please select a character first.");

            // Se há personagem selecionado, fazer validações adicionais
            if (hasCurrentCharacter)
            {
                var character = await currentCharacterService.GetDtoAsync(cancellationToken);
                
                // TODO: Verificar se personagem está online (se requerido)
                if (!attribute.AllowOfflineCharacter)
                {
                }

                // Verificar nível mínimo do personagem
                if (attribute.MinCharacterLevel > 0)
                    if (character.Level < attribute.MinCharacterLevel)
                        throw new ForbiddenException($"CHARACTER_LEVEL_REQUIRED: Character level {attribute.MinCharacterLevel} required. Current level: {character.Level}");

                logger.LogDebug("Character validation passed for user {UserId}, character {CharacterId} ({CharacterName})", 
                    user.Id, character, character.Name);
            }

            logger.LogInformation("Character session check passed for user {UserId}, AllowNoCharacterSelected: {AllowNoCharacter}, AllowOfflineCharacter: {AllowOffline}, MinLevel: {MinLevel}",
                user.Id, attribute.AllowNoCharacterSelected, attribute.AllowOfflineCharacter, attribute.MinCharacterLevel);
        }

        return await next(cancellationToken);
    }
}
