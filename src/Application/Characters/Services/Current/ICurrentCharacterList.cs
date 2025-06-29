using GameServer.Application.Characters.Queries.Models;

namespace GameServer.Application.Characters.Services.Current;

public interface ICurrentCharacterList
{
    Task<List<CharacterSummaryDto>> ListAsync(CancellationToken cancellationToken = default);
}
