using ServerGame.Application.Accounts.Models;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Accounts.Services;

public interface IAccountService
{
    Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default);
    Task<Account> GetAsync(string userId, CancellationToken cancellationToken = default);
    Task<Account> CreateAsync(Account account, CancellationToken cancellationToken = default);
    Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default);
    Task PurgeAsync(CancellationToken cancellationToken = default);
}
