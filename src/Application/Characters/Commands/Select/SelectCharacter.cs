using GameServer.Application.Characters.Services;
using GameServer.Application.Characters.Services.Current;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Security;
using GameServer.Application.Session;
using GameServer.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Characters.Commands.Select;

[RequireGameSession]
public record SelectCharacterCommand(long CharacterId) : IRequest<Unit>;

public class SelectCharacterCommandHandler(
    ICurrentCharacterSelector selector,
    ICurrentCharacterSelection selection,
    ILogger<SelectCharacterCommandHandler> logger)
    : IRequestHandler<SelectCharacterCommand, Unit>
{
    public async Task<Unit> Handle(SelectCharacterCommand request, CancellationToken cancellationToken)
    {
        // Seleciona o personagem
        await selector.SelectAsync(
            request.CharacterId,
            cancellationToken
        );
        
        var selectedCharacter = await selection.GetDtoAsync(cancellationToken);

        logger.LogInformation("Character selected for gameplay: {CharacterName} (ID: {CharacterId})",
            selectedCharacter.Name, request.CharacterId);
        return Unit.Value;
    }
}
