using GameServer.Application.Accounts.Services;

namespace GameServer.Application.Accounts.Commands.Delete;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    private readonly ICurrentAccountService _currentAccountService;

    public DeleteAccountCommandValidator(ICurrentAccountService currentAccountService)
    {
        _currentAccountService = currentAccountService ?? throw new ArgumentNullException(nameof(currentAccountService));

        RuleFor(v => v)
            .MustAsync(BeExistsEntity)
            .WithMessage("Account with the specified userId does not exists.")
            .WithErrorCode("NotFound");
    }

    private async Task<bool> BeExistsEntity(DeleteAccountCommand command, CancellationToken cancellationToken)
    {
        return await _currentAccountService.ExistsAsync(cancellationToken);
    }
}
