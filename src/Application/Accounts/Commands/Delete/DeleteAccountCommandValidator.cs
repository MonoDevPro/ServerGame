using GameServer.Application.Accounts.Services;

namespace GameServer.Application.Accounts.Commands.Delete;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    private readonly IAccountService _accountService;
    
    public DeleteAccountCommandValidator(IAccountService accountService)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

        RuleFor(v => v)
            .MustAsync(BeExistsEntity)
            .WithMessage("Account with the specified userId does not exists.")
            .WithErrorCode("NotFound");
    }
    
    private async Task<bool> BeExistsEntity(DeleteAccountCommand command, CancellationToken cancellationToken)
    {
        return await _accountService.ExistsAsync(cancellationToken);
    }
}
