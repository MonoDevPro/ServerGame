using GameServer.Application.Accounts.Services;

namespace GameServer.Application.Accounts.Commands.Login;

public class LoginAccountCommandValidator : AbstractValidator<LoginAccountCommand>
{
    private readonly ICurrentAccountService _currentAccountService;

    public LoginAccountCommandValidator(ICurrentAccountService currentAccountService)
    {
        _currentAccountService = currentAccountService ?? throw new ArgumentNullException(nameof(currentAccountService));

        RuleFor(v => v)
            .MustAsync(BeExistsEntity)
            .WithMessage("Account with the specified userId does not exists.")
            .WithErrorCode("NotFound");
    }

    private async Task<bool> BeExistsEntity(LoginAccountCommand command, CancellationToken cancellationToken)
    {
        return await _currentAccountService.ExistsAsync(cancellationToken);
    }
}
