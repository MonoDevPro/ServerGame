using GameServer.Application.Characters.Queries.Models;
using GameServer.Application.Characters.Services.Current;
using GameServer.Application.Common.Security;

namespace GameServer.Application.Characters.Queries.Get;

[RequireCharacter]
public record GetCurrentCharacter : IRequest<CharacterDto>;

public class GetMyCharactersQueryHandler(
    ICurrentCharacterSelection currentCharacterService) : IRequestHandler<GetCurrentCharacter, CharacterDto>
{
    public async Task<CharacterDto> Handle(GetCurrentCharacter request, CancellationToken cancellationToken)
    {
        return await currentCharacterService.GetDtoAsync(cancellationToken);
    }
}
