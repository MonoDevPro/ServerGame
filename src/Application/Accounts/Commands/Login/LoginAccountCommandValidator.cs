using GameServer.Application.Accounts.Services;

namespace GameServer.Application.Accounts.Commands.Login;

public class LoginAccountCommandValidator : AbstractValidator<LoginAccountCommand>
{
    private readonly IAccountService _accountService;

    public LoginAccountCommandValidator(IAccountService accountService)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

        RuleFor(v => v)
            .MustAsync(BeExistsEntity)
            .WithMessage("Account with the specified userId does not exists.")
            .WithErrorCode("NotFound");
    }

    private async Task<bool> BeExistsEntity(LoginAccountCommand command, CancellationToken cancellationToken)
    {
        return await _accountService.ExistsAsync(cancellationToken);
    }
}
