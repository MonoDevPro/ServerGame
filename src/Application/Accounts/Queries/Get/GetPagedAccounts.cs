using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Application.Common.Security;
using GameServer.Domain.Constants;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Queries.Get;

[Authorize(Roles = Roles.Administrator)]
public record GetPagedAccountsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null,
    string? AccountType = null
) : IRequest<IPagedList<AccountDto>>;

public class GetPagedAccountsQueryHandler(IReaderRepository<Account> accountRepository, IMapper mapper)
    : IRequestHandler<GetPagedAccountsQuery, IPagedList<AccountDto>>
{
    public async Task<IPagedList<AccountDto>> Handle(GetPagedAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await accountRepository.QueryPagedListAsync(
            pageIndex: request.PageNumber,
            pageSize: request.PageSize,
            predicate: a =>
                (string.IsNullOrEmpty(request.SearchTerm)) &&
                (!request.IsActive.HasValue || a.IsActive == request.IsActive.Value) &&
                (string.IsNullOrEmpty(request.AccountType) ||
                 a.AccountType.ToString().ToLower() == request.AccountType.ToLower()),
            orderBy: a => a.OrderByDescending(x => x.Created),
            selector: a => mapper.Map<AccountDto>(a),
            cancellationToken: cancellationToken
        );

        return accounts;
    }
}
