namespace GameServer.Application.Characters.Services.Current;

public interface ICurrentCharacterSelector
{
    Task SelectAsync(long characterId, CancellationToken cancellationToken = default);
    Task<bool> IsOwnerAsync(long characterId, CancellationToken cancellationToken = default);
}
