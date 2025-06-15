using ServerGame.Application.Accounts.Models;
using ServerGame.Application.Accounts.Services;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Accounts.Queries.GetAccount;

public record GetAccountInfoQuery : IRequest<AccountDto>;

public class GetAccountInfoQueryHandler : IRequestHandler<GetAccountInfoQuery, AccountDto>
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly IAccountService _accountService;
    private readonly IMapper _mapper;

    public GetAccountInfoQueryHandler(
        IUser user,
        IIdentityService identityService,
        IAccountService accountService,
        IMapper mapper)
    {
        _user = user;
        _identityService = identityService;
        _accountService = accountService;
        _mapper = mapper;
    }

    public async Task<AccountDto> Handle(GetAccountInfoQuery request, CancellationToken cancellationToken)
    {
        var usernameValue = await _identityService.GetUserNameAsync(_user.Id!);
        
        Guard.Against.Null(usernameValue, nameof(usernameValue),
            "User ID is null or user name could not be retrieved.");
        try
        {
            var username = Username.Create(usernameValue);
            var account = await _accountService.GetAsync(username, cancellationToken);
            Guard.Against.Null(account, nameof(account), 
                $"Account with username '{usernameValue}' not found.");
            
            return _mapper.Map<AccountDto>(account);
            
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving account for user '{usernameValue}': {ex.Message}", ex);
        }
    }
}
