using GameServer.Application.Characters.Queries.Models;
using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Security;

namespace GameServer.Application.Characters.Queries.Get;

[RequireGameSession]
public record GetCharacterQuery(long CharacterId) : IRequest<CharacterDto>;

public class GetCharacterQueryHandler(
    ICurrentCharacterService currentCharacterService) : IRequestHandler<GetCharacterQuery, CharacterDto>
{
    public async Task<CharacterDto> Handle(GetCharacterQuery request, CancellationToken cancellationToken)
    {
        return await currentCharacterService.GetDtoAsync(cancellationToken);
    }
}
