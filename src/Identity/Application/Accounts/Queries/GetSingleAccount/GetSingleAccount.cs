using System.ComponentModel.DataAnnotations;
using ServerGame.Application.Accounts.Models;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Models;
using ServerGame.Domain.Entities;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.Accounts.Queries.GetSingleAccount;

public record GetSingleAccountQuery(
    UsernameOrEmail UsernameOrEmail
) : IRequest<AccountDto>;

public class GetSingleAccountQueryHandler : IRequestHandler<GetSingleAccountQuery, AccountDto>
{
    private readonly IReaderRepository<Account> _readerAccountRepository;
    private readonly IMapper _mapper;

    public GetSingleAccountQueryHandler(IReaderRepository<Account> readerAccountRepository, IMapper mapper)
    {
        _readerAccountRepository = readerAccountRepository;
        _mapper = mapper;
    }

    public async Task<AccountDto> Handle(GetSingleAccountQuery request, CancellationToken cancellationToken)
    {
        // 1) Extraia a string normalizada (trim+lower) do VO:
        string key = request.UsernameOrEmail.Value;

        // 2) Monte a expressão para o EF (que compara duas strings):
        AccountDto? accountDto = await _readerAccountRepository
            .QuerySingleAsync(
                // filtro: compara as colunas Username ou Email
                predicate: a => a.Username == key 
                                || a.Email == key,
                selector: account => _mapper.Map<AccountDto>(account),
                include: accounts => accounts
                    .Include(a => a.Roles),
                cancellationToken: cancellationToken);

        // 3) Falha rápida se não encontrou:
        Guard.Against.Null(
            accountDto, 
            nameof(accountDto),
            $"Account with username or email '{key}' not found.");

        return accountDto!;
    }
}
