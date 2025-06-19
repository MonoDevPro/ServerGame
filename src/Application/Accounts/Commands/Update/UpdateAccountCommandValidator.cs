using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Commands.Update;

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    private readonly IReaderRepository<Account> _repository;
    public UpdateAccountCommandValidator(IReaderRepository<Account> repository)
    {
        _repository = repository;
        
        RuleFor(v => v)
            .NotEmpty()
            .MustAsync(BeExistsEntity)
            .WithMessage("Account with the specified UserId does not exist.")
            .WithErrorCode("NotFound");
    }
    
    private async Task<bool> BeExistsEntity(UpdateAccountCommand command, CancellationToken cancellationToken)
    {
        return await _repository.ExistsAsync(
            a => a.CreatedBy == command.UserId,
            cancellationToken
        );
    }
}
