using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Accounts.Services;

public interface IAccountService
{
    Task<bool> ExistsAsync(UsernameOrEmail usernameOrEmail, CancellationToken cancellationToken = default);
    Task<Account> GetAsync(Username username, CancellationToken cancellationToken = default);
    Task<Account> GetAsync(Email email, CancellationToken cancellationToken = default);
    Task<Account> CreateAsync(Account account, CancellationToken cancellationToken = default);
    Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default);
}
