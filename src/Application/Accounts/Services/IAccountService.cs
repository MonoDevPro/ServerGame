using GameServer.Application.Accounts.Queries.Models;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Services;

public interface IAccountService
{
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);
    Task<Account> GetReadOnlyAsync(CancellationToken cancellationToken = default);
    Task<Account> GetForUpdateAsync(CancellationToken cancellationToken = default);
    Task<AccountDto> GetDtoAsync(CancellationToken cancellationToken = default);
    Task<Account> CreateAsync(CancellationToken cancellationToken = default);
    Task PurgeAsync(CancellationToken cancellationToken = default);
}
