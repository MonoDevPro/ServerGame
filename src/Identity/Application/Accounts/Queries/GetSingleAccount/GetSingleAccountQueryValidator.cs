using System.Text.RegularExpressions;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Accounts.Queries.GetSingleAccount;

public class GetSingleAccountQueryValidator 
    : AbstractValidator<GetSingleAccountQuery>
{
    private readonly IReaderRepository<Account> _accountRepository;
    public GetSingleAccountQueryValidator(
        IReaderRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;

        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("O campo UsernameOrEmail é obrigatório.")
            .MustAsync(BeExistsEntity).WithMessage("O campo UsernameOrEmail deve ser um nome de usuário ou email válido.");
    }
    
    private async Task<bool> BeExistsEntity(GetSingleAccountQuery query, UsernameOrEmail usernameOrEmail, CancellationToken cancellationToken)
    {
        return await _accountRepository.ExistsAsync(
            a => a.Email == usernameOrEmail,
            cancellationToken
        );
    }
}
