using ServerGame.Application.Accounts.Models;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Entities.Accounts;

namespace ServerGame.Application.Accounts.Queries.GetAccountsList;

public record GetAccountsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null,
    string? AccountType = null
) : IRequest<IPagedList<AccountDto>>;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, IPagedList<AccountDto>>
{
    private readonly IReaderRepository<Account> _accountRepository;
    private readonly IMapper _mapper;

    public GetAccountsQueryHandler(IReaderRepository<Account> accountRepository, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<IPagedList<AccountDto>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.QueryPagedListAsync(
            pageIndex: request.PageNumber,
            pageSize: request.PageSize,
            predicate: a => 
                (string.IsNullOrEmpty(request.SearchTerm) || 
                 a.Username.Value.Contains(request.SearchTerm) || 
                 a.Email.Value.Contains(request.SearchTerm)) &&
                (!request.IsActive.HasValue || a.IsActive == request.IsActive.Value) &&
                (string.IsNullOrEmpty(request.AccountType) || 
                 a.AccountType.ToString().ToLower() == request.AccountType.ToLower()),
            orderBy: a => a.OrderByDescending(x => x.Created),
            selector: a => _mapper.Map<AccountDto>(a),
            cancellationToken: cancellationToken
        );

        return accounts;
    }
}
