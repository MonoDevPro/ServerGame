using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;

namespace GameServer.Infrastructure.Services.Accounts;

/// <summary>
/// Serviço que representa a conta associada ao usuário atual.
/// </summary>
public class CurrentAccountService(
    IUser user,
    IAccountService account
    ) : ICurrentAccountService
{
    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
            return false;

        return await account.ExistsAsync(user.Id, cancellationToken);
    }
    
    public async Task<long> GetIdAsync(CancellationToken cancellationToken = default)
    {
        if (!await ExistsAsync(cancellationToken))
            throw new NotFoundException("No account for current user", nameof(user.Id));

        var dto = await account.GetDtoAsync(user.Id!, cancellationToken);
        return dto.Id;
    }

    public async Task<AccountDto> GetDtoAsync(CancellationToken cancellationToken = default)
    {
        if (!await ExistsAsync(cancellationToken))
            throw new NotFoundException("No account for current user", nameof(user.Id));

        return await account.GetDtoAsync(user.Id!, cancellationToken);
    }

    public async Task<AccountDto> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        if (!await ExistsAsync(cancellationToken))
            await account.CreateAsync(user.Id!, cancellationToken);

        return await GetDtoAsync(cancellationToken);
    }

    public async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        if (await ExistsAsync(cancellationToken))
            await account.PurgeAsync(user.Id!, cancellationToken);
    }
}
