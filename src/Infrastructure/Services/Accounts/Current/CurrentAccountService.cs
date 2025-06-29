using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Accounts.Services.Current;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Interfaces.Persistence;

namespace GameServer.Infrastructure.Services.Accounts.Current;

/// <summary>
/// Serviço que representa a Account do usuário atual (contexto “meu usuário”).
/// </summary>
public class CurrentAccountService(
    IUser user,
    IAccountQueryService query,
    IAccountCommandService command,
    IUnitOfWork unitOfWork
    ) : ICurrentAccountService
{
    
    private string UserId => user.Id ?? throw new InvalidOperationException("User ID is not set");
    
    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(UserId))
            return false;

        return await query.ExistsAsync(UserId, cancellationToken);
    }

    public async Task<long> GetIdAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(UserId))
            return 0;

        return await query.GetIdAsync(UserId, cancellationToken);
    }

    public async Task<AccountDto> GetDtoAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(UserId))
            throw new InvalidOperationException("User ID is not set");
        
        return await query.GetDtoAsync(UserId, cancellationToken);
    }

    public async Task<AccountDto> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        if (await ExistsAsync(cancellationToken))
            return await GetDtoAsync(cancellationToken);

        // Criar a conta usando o UserId
        await command.CreateAsync(UserId, cancellationToken);
        // Persistir as alterações
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Retornar o DTO da conta
        return await GetDtoAsync(cancellationToken);
    }

    public async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        if (!await ExistsAsync(cancellationToken))
            return;

        var id = await GetIdAsync(cancellationToken);
        await command.PurgeAsync(id, cancellationToken);
    }
}
