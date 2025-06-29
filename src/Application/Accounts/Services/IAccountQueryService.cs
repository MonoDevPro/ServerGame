using GameServer.Application.Accounts.Queries.Models;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Services;

public interface IAccountQueryService
{
    Task<bool> ExistsAsync(long accountId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default);
    Task<Account> GetByIdAsync(long accountId, CancellationToken cancellationToken = default);
    Task<long> GetIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<AccountDto> GetDtoAsync(long accountId, CancellationToken cancellationToken = default);
    Task<AccountDto> GetDtoAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> HasAnyCharacterAsync(long accountId, CancellationToken cancellationToken = default);
    Task<bool> IsOwnerAsync(long accountId, string userId, CancellationToken cancellationToken = default);
}
