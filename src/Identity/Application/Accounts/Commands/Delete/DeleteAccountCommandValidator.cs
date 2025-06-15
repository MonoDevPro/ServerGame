using ServerGame.Application.Common.Interfaces.Persistence.Repository;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Accounts.Commands.Delete;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    private readonly IReaderRepository<Account> _repository;
    
    public DeleteAccountCommandValidator(IReaderRepository<Account> repository)
    {
        _repository = repository;

        RuleFor(v => v.UsernameOrEmail)
            .NotEmpty()
            .MustAsync(BeExistsEntity)
            .WithMessage("Account with the specified username or email does not exist.")
            .WithErrorCode("NotFound");
    }
    
    public async Task<bool> BeExistsEntity(DeleteAccountCommand command, UsernameOrEmail usernameOrEmail, CancellationToken cancellationToken)
    {
        return await _repository.ExistsAsync(a => 
                a.Email        == usernameOrEmail 
                 || a.Username == usernameOrEmail,
                                     cancellationToken
        );
    }
}
