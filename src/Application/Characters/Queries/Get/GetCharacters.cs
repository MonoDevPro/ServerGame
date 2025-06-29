using GameServer.Application.Characters.Queries.Models;
using GameServer.Application.Characters.Services.Current;
using GameServer.Application.Common.Security;
using GameServer.Domain.Enums;

namespace GameServer.Application.Characters.Queries.Get;

[RequireGameSession]
public record GetAccountCharactersQuery : IRequest<List<CharacterSummaryDto>>;

public class GetAccountCharactersQueryHandler(
    ICurrentCharacterList list) : IRequestHandler<GetAccountCharactersQuery, List<CharacterSummaryDto>>
{
    public async Task<List<CharacterSummaryDto>> Handle(GetAccountCharactersQuery request, CancellationToken cancellationToken)
    {
        return await list.ListAsync(cancellationToken);
    }
}
