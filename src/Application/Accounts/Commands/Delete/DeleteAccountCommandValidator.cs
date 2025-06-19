using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Commands.Delete;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    private readonly IReaderRepository<Account> _repository;
    
    public DeleteAccountCommandValidator(IReaderRepository<Account> repository)
    {
        _repository = repository;

        RuleFor(v => v)
            .NotEmpty()
            .MustAsync(BeExistsEntity)
            .WithMessage("Account with the specified userId does not exists.")
            .WithErrorCode("NotFound");
    }
    
    public async Task<bool> BeExistsEntity(DeleteAccountCommand command, CancellationToken cancellationToken)
    {
        return await _repository.ExistsAsync(a => 
                a.CreatedBy        == command.userId ,
                cancellationToken
        );
    }
}
