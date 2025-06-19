using GameServer.Application.Accounts.Commands.Create;
using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Domain.Exceptions;

namespace GameServer.Application.Accounts.Queries.GetAccount;

public record GetAccountQuery : IRequest<AccountDto>;

public class GetAccountQueryHandler(
    IUser user,
    IAccountService accountService,
    IMapper mapper,
    IMediator mediator)
    : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var userId = user.Id;
        Guard.Against.Null(userId, nameof(userId),
            "User ID is null or user name could not be retrieved.", () => new Exception("User ID is null or user name could not be retrieved."));
        try
        {
            
            var accountExists = await accountService.ExistsAsync(userId, cancellationToken);
            if (!accountExists)
            {
                // Se a conta não existir, vamos criar uma nova
                var createAccountCommand = new CreateAccountCommand();
                await mediator.Send(createAccountCommand, cancellationToken);
            }
            
            var account = await accountService.GetAsync(userId, cancellationToken);
            
            // Se mesmo após tentar criar ainda está nulo, então realmente há um problema
            Guard.Against.Null(account, nameof(account), 
                $"Failed to create account for user id '{userId}'.", 
                () => new DomainException($"Failed to create account for user id '{userId}'."));
            
            return mapper.Map<AccountDto>(account);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving account for user '{userId}': {ex.Message}", ex);
        }
    }
}
