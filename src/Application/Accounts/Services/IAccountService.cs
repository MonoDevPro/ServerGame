using GameServer.Application.Accounts.Queries.Models;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Services;

public interface IAccountService
{
    Task<bool> ExistsAsync(long accountId, CancellationToken cancellationToken = default);
    Task<Account> GetByIdAsync(long accountId, CancellationToken cancellationToken = default);
    Task<Account> CreateAsync(string userId, CancellationToken cancellationToken = default);
    Task<AccountDto> GetDtoAsync(long accountId, CancellationToken cancellationToken = default);
    Task<Account> GetForUpdateAsync(long accountId, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccountDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(long accountId, CancellationToken cancellationToken = default);
    Task PurgeAsync(long accountId, CancellationToken cancellationToken = default);
    Task<bool> CanCreateCharacterAsync(long accountId, CancellationToken cancellationToken = default);
    Task<bool> HasAnyCharacterAsync(long accountId, CancellationToken cancellationToken = default);
    Task<bool> IsOwnerAsync(long accountId, string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default);
    Task<AccountDto> GetDtoAsync(string userId, CancellationToken cancellationToken = default);
    Task<Account> CreateAsync(CancellationToken cancellationToken = default);
    Task PurgeAsync(string userId, CancellationToken cancellationToken = default);
}
