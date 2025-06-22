using AutoMapper;
using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;
using GameServer.Domain.Exceptions;

namespace GameServer.Infrastructure.Services;

public class AccountService(
    IUser user,
    IMapper mapper,
    IRepositoryCompose<Account> accountRepository)
    : IAccountService
{
    private readonly IRepositoryCompose<Account> _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    private IReaderRepository<Account> ReaderAccountRepository => _accountRepository.ReaderRepository;
    private IWriterRepository<Account> WriterAccountRepository => _accountRepository.WriterRepository;

    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        return await ReaderAccountRepository
            .ExistsAsync(a => a.CreatedBy == user.Id,
                cancellationToken: cancellationToken);
    }

    public async Task<Account> GetReadOnlyAsync(CancellationToken cancellationToken = default) =>
        await ReaderAccountRepository
            .QuerySingleAsync<Account>(
                predicate: a => a.CreatedBy == user.Id,
                trackingType: TrackingType.NoTracking,
                cancellationToken: cancellationToken)
        ?? throw new KeyNotFoundException($"Account with userId '{user.Id}' not found.");
    public async Task<Account> GetForUpdateAsync(CancellationToken cancellationToken = default) =>
        await ReaderAccountRepository
            .QuerySingleAsync<Account>(
                predicate: a => a.CreatedBy == user.Id,
                trackingType: TrackingType.Tracking,
                cancellationToken: cancellationToken)
        ?? throw new KeyNotFoundException($"Account with userId '{user.Id}' not found.");

    public async Task<AccountDto> GetDtoAsync(CancellationToken cancellationToken = default)
    {
        return await ReaderAccountRepository
                   .QuerySingleAsync<AccountDto>(
                       predicate: a => a.CreatedBy == user.Id,
                       selector: a => mapper.Map<AccountDto>(a),
                       trackingType: TrackingType.NoTracking,
                       cancellationToken: cancellationToken)
               ?? throw new NotFoundException(
                   user.Id ?? String.Empty,
                   nameof(user.Id),
                   new DomainException($"Account with userId '{user.Id}' not found."));
    }

    public async Task<Account> CreateAsync(CancellationToken cancellationToken = default)
        => await WriterAccountRepository.AddAsync(Account.Create(), cancellationToken);

    public async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await ReaderAccountRepository
            .QueryListAsync<Account>(
                trackingType: TrackingType.Tracking,
                cancellationToken: cancellationToken);

        accounts.ForEach(a => a.Deactivate());
    }

    private async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
    {
        return await WriterAccountRepository.SaveChangesAsync(cancellationToken);
    }
}
