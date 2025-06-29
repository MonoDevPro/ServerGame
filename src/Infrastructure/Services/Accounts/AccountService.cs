using AutoMapper;
using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;

namespace GameServer.Infrastructure.Services.Accounts;

public class AccountService(
    IMapper mapper,
    IRepositoryCompose<Account> repo): IAccountQueryService, IAccountCommandService
{
    public async Task<bool> ExistsAsync(long accountId, CancellationToken cancellationToken = default)
    {
        return await repo.ReaderRepository.ExistsAsync(
            a => a.Id == accountId, 
            cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return await repo.ReaderRepository.ExistsAsync(
            a => a.CreatedBy == userId, 
            cancellationToken: cancellationToken);
    }

    public async Task<Account> GetByIdAsync(long accountId, CancellationToken cancellationToken = default)
    {
        return await repo.ReaderRepository.QuerySingleAsync<Account>(
            predicate: a => a.Id == accountId,
            trackingType: TrackingType.Tracking,
            cancellationToken: cancellationToken) ?? throw new NotFoundException(nameof(accountId), $"Account with ID {accountId} not found");
    }

    public Task<long> GetIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        return repo.ReaderRepository.QuerySingleValueAsync(
            predicate: a => a.CreatedBy == userId,
            selector: a => a.Id,
            trackingType: TrackingType.NoTracking,
            cancellationToken: cancellationToken) ?? throw new NotFoundException(nameof(userId), $"Account for user {userId} not found");
    }

    public async Task<AccountDto> GetDtoAsync(long accountId, CancellationToken cancellationToken = default)
    {
        return await repo.ReaderRepository
            .QuerySingleAsync<AccountDto>(
                predicate: a => a.Id == accountId,
                selector: a => mapper.Map<AccountDto>(a),
                trackingType: TrackingType.NoTracking,
                cancellationToken: cancellationToken) ?? throw new NotFoundException(nameof(accountId), $"Account with ID {accountId} not found");
    }

    public async Task<AccountDto> GetDtoAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        return await repo.ReaderRepository.QuerySingleAsync<AccountDto>(
            predicate: a => a.CreatedBy == userId,
            selector: a => mapper.Map<AccountDto>(a),
            trackingType: TrackingType.NoTracking,
            cancellationToken: cancellationToken) ?? throw new NotFoundException(nameof(userId), $"Account for user {userId} not found");
    }

    public async Task<Account> CreateAsync(CancellationToken cancellationToken = default)
    {
        var account = Account.Create();
        return await repo.WriterRepository.AddAsync(account, cancellationToken);
    }

    public async Task<Account> CreateAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        if (await ExistsAsync(userId, cancellationToken))
            throw new InvalidOperationException($"Account already exists for user {userId}");

        var account = Account.Create();
        return await repo.WriterRepository.AddAsync(account, cancellationToken);
    }

    public async Task UpdateAsync(AccountDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var entity = await GetByIdAsync(dto.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException(nameof(dto.Id), $"Account with ID {dto.Id} not found");

        mapper.Map(dto, entity);
    }

    public async Task DeleteAsync(long accountId, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(accountId, cancellationToken);
        if (entity == null)
            throw new NotFoundException(nameof(accountId), $"Account with ID {accountId} not found");

        await repo.WriterRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task PurgeAsync(long accountId, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(accountId, cancellationToken);
        if (entity == null)
            throw new NotFoundException(nameof(accountId), $"Account with ID {accountId} not found");

        entity.Deactivate();
    }

    public async Task PurgeAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return;

        var entity = await repo.ReaderRepository
            .QuerySingleAsync<Account>(
                predicate: a => a.CreatedBy == userId,
                trackingType: TrackingType.Tracking,
                cancellationToken: cancellationToken);

        if (entity != null)
            entity.Deactivate();
    }

    public async Task<bool> HasAnyCharacterAsync(long accountId, CancellationToken cancellationToken = default)
    {
        return await repo.ReaderRepository.ExistsAsync(
            a => a.Id == accountId && a.Characters.Any(),
            cancellationToken: cancellationToken);
    }

    public async Task<bool> IsOwnerAsync(long accountId, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return await repo.ReaderRepository.ExistsAsync(
            a => a.Id == accountId && a.CreatedBy == userId,
            cancellationToken: cancellationToken);
    }
}
