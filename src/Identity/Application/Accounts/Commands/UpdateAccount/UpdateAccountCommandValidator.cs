using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Domain.Entities;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.Accounts.Commands.UpdateAccount;

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    private readonly IReaderRepository<Account> _repository;
    public UpdateAccountCommandValidator(IReaderRepository<Account> repository)
    {
        _repository = repository;
        
        RuleFor(v => v.Id)
            .NotEmpty()
            .MustAsync(BeExistsEntity)
            .WithMessage("Account with the specified ID does not exist.")
            .WithErrorCode("NotFound");

        RuleFor(v => v.Email)
            .NotEmpty()
            .MustAsync(BeUniqueEmail);
        RuleFor(v => v.Username)
            .NotEmpty()
            .MustAsync(BeUniqueUsername);
    }
    
    private async Task<bool> BeExistsEntity(UpdateAccountCommand command, long id, CancellationToken cancellationToken)
    {
        return await _repository.ExistsAsync(
            a => a.Id == id,
            cancellationToken
        );
    }
    
    private async Task<bool> BeUniqueUsername(UpdateAccountCommand command, Username username, CancellationToken cancellationToken)
    {
        return !await _repository.ExistsAsync(
            a => a.Username == username && a.Id != command.Id,
            cancellationToken
        );
    }
    
    private async Task<bool> BeUniqueEmail(UpdateAccountCommand command, Email email, CancellationToken cancellationToken)
    {
        return !await _repository.ExistsAsync(
            a => a.Email == email && a.Id != command.Id,
            cancellationToken
        );
    }
}
