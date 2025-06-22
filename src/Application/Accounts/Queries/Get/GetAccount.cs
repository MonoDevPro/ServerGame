using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;

namespace GameServer.Application.Accounts.Queries.Get;

public record GetAccountQuery : IRequest<AccountDto>;

public class GetAccountQueryHandler(
    IAccountService accountService) : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken ct)
    {
        try
        {
            return await accountService.GetDtoAsync(ct);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving account: {ex.Message}", ex);
        }
    }
}

public class GetAccountQueryValidator : AbstractValidator<GetAccountQuery>
{
    private readonly IAccountService _accountService;

    public GetAccountQueryValidator(IAccountService accountService)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

        RuleFor(v => v)
            .MustAsync(BeExistsEntity)
            .WithMessage("Account does not exist.")
            .WithErrorCode("NotFound");
    }

    private async Task<bool> BeExistsEntity(GetAccountQuery query, CancellationToken cancellationToken)
    {
        return await _accountService.ExistsAsync(cancellationToken);
    }
}