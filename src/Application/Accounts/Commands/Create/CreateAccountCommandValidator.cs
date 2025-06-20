using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Commands.Create;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    private readonly IAccountService _accountService;
    
    public CreateAccountCommandValidator(
        IAccountService accountService)
    {
        _accountService = accountService;
        
        RuleFor(x => x)
            .MustAsync(BeUniqueForUser)  .WithMessage("'{PropertyName}' must be unique.")
                                         .WithErrorCode("Unique");
    }
    
    private async Task<bool> BeUniqueForUser(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        return !await _accountService.ExistsAsync(cancellationToken);
    }
}
