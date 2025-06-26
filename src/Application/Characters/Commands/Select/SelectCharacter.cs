using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Security;
using GameServer.Application.Session;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Characters.Commands.Select;

[RequireCharacter]
public record SelectCharacterCommand(long CharacterId) : IRequest<SelectCharacterResult>;

public record SelectCharacterResult(
    bool Success,
    string? CharacterName,
    string? ErrorMessage
);

public class SelectCharacterCommandHandler(
    ICurrentCharacterService currentCharacterService,
    IGameSessionService sessionService,
    IUser user,
    ILogger<SelectCharacterCommandHandler> logger)
    : IRequestHandler<SelectCharacterCommand, SelectCharacterResult>
{
    public async Task<SelectCharacterResult> Handle(SelectCharacterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verifica se o personagem existe e pertence ao usuário
            if (!await currentCharacterService.IsCharacterOwnerAsync(request.CharacterId, cancellationToken))
            {
                return new SelectCharacterResult(false, null, "Character not found or you don't have permission to select it");
            }

            // Obtém o personagem
            var character = await currentCharacterService.GetDtoAsync(cancellationToken);

            // Define o personagem selecionado na sessão de jogo
            await sessionService.SetSelectedCharacterAsync(user.Id!, request.CharacterId);

            logger.LogInformation("Character selected for gameplay: {CharacterName} (ID: {CharacterId})",
                character.Name, request.CharacterId);

            return new SelectCharacterResult(true, character.Name, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error selecting character: {CharacterId}", request.CharacterId);
            return new SelectCharacterResult(false, null, ex.Message);
        }
    }
}
