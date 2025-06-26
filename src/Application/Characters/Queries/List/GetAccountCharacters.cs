using GameServer.Application.Characters.Queries.Models;
using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Security;

namespace GameServer.Application.Characters.Queries.List;

[RequireGameSession]
public record GetAccountCharactersQuery : IRequest<List<CharacterSummaryDto>>;

public class GetAccountCharactersQueryHandler(
    ICurrentCharacterService currentCharacterService) : IRequestHandler<GetAccountCharactersQuery, List<CharacterSummaryDto>>
{
    public async Task<List<CharacterSummaryDto>> Handle(GetAccountCharactersQuery request, CancellationToken cancellationToken)
    {
        return await currentCharacterService.GetAccountCharactersAsync(cancellationToken);
    }
}
