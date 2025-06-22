using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Domain.Exceptions;

namespace GameServer.Application.Accounts.Queries.Get;

public record GetAccountQuery : IRequest<AccountDto>;

public class GetAccountQueryHandler(
    IAccountService accountService) : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken ct)
    {
        try
        {
            if (await accountService.ExistsAsync(ct) == false)
                throw new UnauthorizedAccessException("Account not found. Please login first.");

            return await accountService.GetDtoAsync(ct);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving account: {ex.Message}", ex);
        }
    }
}
