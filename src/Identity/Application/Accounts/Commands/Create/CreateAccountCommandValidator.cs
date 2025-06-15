using ServerGame.Application.Common.Interfaces.Persistence.Repository;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Accounts.Commands.Create;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    private readonly IReaderRepository<Account> _repository;
    
    public CreateAccountCommandValidator(
        IReaderRepository<Account> repository)
    {
        _repository = repository;
        
        RuleFor(v => v.Email)
            .NotEmpty()
            .MustAsync(BeUniqueEmail)
            .WithMessage("'{PropertyName}' must be unique.")
            .WithErrorCode("Unique");
        RuleFor(v => v.Username)
            .NotEmpty()
            .MustAsync(BeUniqueUsername)
            .WithMessage("'{PropertyName}' must be unique.")
            .WithErrorCode("Unique");
    }
    
    private async Task<bool> BeUniqueUsername(Username username, CancellationToken cancellationToken)
    {
        return !await _repository.ExistsAsync(a => a.Username == username, cancellationToken);
    }
    
    private async Task<bool> BeUniqueEmail(Email email, CancellationToken cancellationToken)
    {
        return !await _repository.ExistsAsync(a => a.Email == email, cancellationToken);
    }
}
