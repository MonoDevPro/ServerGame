using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Security;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Characters.Commands.Delete;

[RequireCharacter(AllowNoCharacterSelected = true)]
public record DeleteCharacterCommand(long CharacterId) : IRequest<DeleteCharacterResult>;

public record DeleteCharacterResult(
    bool Success,
    string? ErrorMessage
);

public class DeleteCharacterCommandHandler(
    ICurrentCharacterService currentCharacterService,
    ILogger<DeleteCharacterCommandHandler> logger)
    : IRequestHandler<DeleteCharacterCommand, DeleteCharacterResult>
{
    public async Task<DeleteCharacterResult> Handle(DeleteCharacterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verifica se o personagem existe e pertence ao usuário
            if (!await currentCharacterService.IsCharacterOwnerAsync(request.CharacterId, cancellationToken))
            {
                return new DeleteCharacterResult(false, "Character not found or you don't have permission to delete it");
            }

            await currentCharacterService.DeleteAsync(request.CharacterId, cancellationToken);

            logger.LogInformation("Character deleted successfully: {CharacterId}", request.CharacterId);

            return new DeleteCharacterResult(true, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting character: {CharacterId}", request.CharacterId);
            return new DeleteCharacterResult(false, ex.Message);
        }
    }
}
