using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Security;

namespace GameServer.Application.Accounts.Queries.Get;

[RequireGameSession]
public record GetAccountQuery : IRequest<AccountDto>;

public class GetAccountQueryHandler(
    IAccountService accountService) : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken ct)
    {
        return await accountService.GetDtoAsync(ct);
    }
}
