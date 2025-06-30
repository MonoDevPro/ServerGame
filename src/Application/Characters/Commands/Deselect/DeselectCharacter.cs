using GameServer.Application.Characters.Services.Current;
using GameServer.Application.Common.Security;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Characters.Commands.Deselect;

/// <summary>
/// Comando para fazer logout/desseleção do personagem atual
/// </summary>
[RequireCharacter(AllowNotOwner = false)]
public record DeselectCharacterCommand : IRequest<Unit>;

public class DeselectCharacterCommandHandler(
    ICurrentCharacterSelection selection,
    ILogger<DeselectCharacterCommandHandler> logger)
    : IRequestHandler<DeselectCharacterCommand, Unit>
{
    public async Task<Unit> Handle(DeselectCharacterCommand request, CancellationToken cancellationToken)
    {
        // Obter informações do personagem antes de desselecionar para logs
        var character = await selection.GetDtoAsync(cancellationToken);
        
        // Fazer logout do personagem (remove apenas a seleção)
        await selection.LogoutAsync(cancellationToken);
        
        logger.LogInformation("Character deselected: {CharacterName} (ID: {CharacterId})", 
            character.Name, character.Id);
            
        return Unit.Value;
    }
}
