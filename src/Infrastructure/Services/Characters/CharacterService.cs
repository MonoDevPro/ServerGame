using AutoMapper;
using GameServer.Application.Characters.Queries.Models;
using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;
using GameServer.Domain.Enums;

namespace GameServer.Infrastructure.Services.Characters;

// 3. Implementação genérica de Character (ICharacterService)
public class CharacterService(
    IMapper mapper,
    IRepositoryCompose<Character> repo) : ICharacterService
{
    public async Task<bool> ExistsAsync(long characterId, CancellationToken cancellationToken = default)
    {
        return await repo.ReaderRepository.ExistsAsync(
            c => c.Id == characterId, 
            cancellationToken: cancellationToken);
    }

    public async Task<List<CharacterSummaryDto>> GetAccountCharactersAsync(long accountId, CancellationToken cancellationToken = default)
    {
        return await repo.ReaderRepository
            .QueryListAsync<CharacterSummaryDto>(
                predicate: c => c.AccountId == accountId,
                selector: c => mapper.Map<CharacterSummaryDto>(c),
                trackingType: TrackingType.NoTracking,
                cancellationToken: cancellationToken);
    }

    public async Task<CharacterDto> GetByIdAsync(long characterId, CancellationToken cancellationToken = default)
    {
        return await repo.ReaderRepository
            .QuerySingleAsync<CharacterDto>(
                predicate: c => c.Id == characterId,
                selector: c => mapper.Map<CharacterDto>(c),
                trackingType: TrackingType.NoTracking,
                cancellationToken: cancellationToken) 
               ?? throw new NotFoundException(nameof(characterId), $"Character with ID {characterId} not found");
    }

    public async Task<Character> CreateAsync(long accountId, string name, CharacterClass characterClass, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Character name cannot be null or empty", nameof(name));

        if (!await CanCreateCharacterAsync(accountId, cancellationToken))
            throw new InvalidOperationException("Account has reached maximum character limit");

        var entity = Character.Create(accountId, name, characterClass);
        return await repo.WriterRepository.AddAsync(entity, cancellationToken);
    }

    public async Task<Character?> GetForUpdateAsync(long characterId, CancellationToken cancellationToken = default)
    {
        return await repo.ReaderRepository
            .QuerySingleAsync<Character>(
                predicate: c => c.Id == characterId,
                trackingType: TrackingType.Tracking,
                cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(CharacterDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var entity = await GetForUpdateAsync(dto.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException($"{nameof(dto.Id)}", $"Character with ID {dto.Id} not found");

        mapper.Map(dto, entity);
        //await repo.WriterRepository.UpdateAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(long characterId, CancellationToken cancellationToken = default)
    {
        var entity = await GetForUpdateAsync(characterId, cancellationToken);
        if (entity == null)
            throw new NotFoundException($"{nameof(characterId)}", $"Character with ID {characterId} not found");

        await repo.WriterRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<bool> IsOwnerAsync(long characterId, long accountId, CancellationToken cancellationToken = default)
    {
        return await repo.ReaderRepository.ExistsAsync(
            c => c.Id == characterId && c.AccountId == accountId,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> CanCreateCharacterAsync(long accountId, CancellationToken cancellationToken = default)
    {
        const int maxCharactersPerAccount = 3;
        
        var characterCount = await repo.ReaderRepository
            .CountAsync(c => c.AccountId == accountId, cancellationToken: cancellationToken);
            
        return characterCount < maxCharactersPerAccount;
    }
}
