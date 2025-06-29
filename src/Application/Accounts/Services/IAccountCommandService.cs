using GameServer.Application.Accounts.Queries.Models;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Services;

public interface IAccountCommandService
{
    Task<Account> CreateAsync(string userId, CancellationToken cancellationToken = default);
    Task<Account> CreateAsync(CancellationToken cancellationToken = default);       // se realmente precisar
    Task UpdateAsync(AccountDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(long accountId, CancellationToken cancellationToken = default);
    Task PurgeAsync(long accountId, CancellationToken cancellationToken = default);
    Task PurgeAsync(string userId, CancellationToken cancellationToken = default);  // se realmente precisar
}
