using GameServer.Application.Characters.Services;
using GameServer.Application.Characters.Services.Current;
using GameServer.Application.Common.Security;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Characters.Commands.Delete;

[RequireCharacter]
public record DeleteCharacterCommand(long CharacterId) : IRequest<Unit>;

public class DeleteCharacterCommandHandler(
    ICurrentCharacterSelection selection,
    ILogger<DeleteCharacterCommandHandler> logger)
    : IRequestHandler<DeleteCharacterCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCharacterCommand request, CancellationToken cancellationToken)
    {
        await selection.PurgeAsync(cancellationToken);

        logger.LogInformation("Character deleted successfully: {CharacterId}", request.CharacterId);
        return Unit.Value;
    }
}
