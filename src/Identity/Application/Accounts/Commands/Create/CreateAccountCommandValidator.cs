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
        
        RuleFor(v => v.userId)
            .NotEmpty()
            .MustAsync(BeUniqueForUser)
            .WithMessage("'{PropertyName}' must be unique.")
            .WithErrorCode("Unique");
    }
    
    private async Task<bool> BeUniqueForUser(string userId, CancellationToken cancellationToken)
    {
        return !await _repository.ExistsAsync(a => a.CreatedBy == userId, cancellationToken);
    }
}
