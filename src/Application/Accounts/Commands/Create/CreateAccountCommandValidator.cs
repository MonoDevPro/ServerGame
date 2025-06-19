using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Commands.Create;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    private readonly IReaderRepository<Account> _repository;
    
    public CreateAccountCommandValidator(
        IUser user,
        IReaderRepository<Account> repository)
    {
        _repository = repository;
        
        RuleFor(x => user.Id)
            .NotNull()                   .WithMessage("User ID cannot be null.")
            .MustAsync(BeUniqueForUser)  .WithMessage("'{PropertyName}' must be unique.")
                                         .WithErrorCode("Unique");
    }
    
    private async Task<bool> BeUniqueForUser(string? userId, CancellationToken cancellationToken)
    {
        return !await _repository.ExistsAsync(a => a.CreatedBy == userId, cancellationToken);
    }
}
