using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Services;

public interface IAccountService
{
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);
    Task<Account> GetAsync(CancellationToken cancellationToken = default);
    Task<Account> CreateAsync(Account account, CancellationToken cancellationToken = default);
    Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default);
    Task PurgeAsync(CancellationToken cancellationToken = default);
}
