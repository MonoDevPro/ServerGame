using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ServerGame.Application.Accounts.Services;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly IReaderRepository<Account> _readerAccountRepository;
    private readonly IWriterRepository<Account> _writerAccountRepository;

    public AccountService(
        IReaderRepository<Account> readerAccountRepository,
        IWriterRepository<Account> writerAccountRepository
        )
    {
        _readerAccountRepository = readerAccountRepository;
        _writerAccountRepository = writerAccountRepository;
    }

    public async Task<bool> ExistsAsync(UsernameOrEmail usernameOrEmail, CancellationToken cancellationToken = default)
    {
        // 2) Execute a consulta e retorne o resultado:
        return await _readerAccountRepository
            .ExistsAsync(a =>
                a.Username == usernameOrEmail.Username || 
                a.Email == usernameOrEmail.Email, 
                cancellationToken: cancellationToken);
    }

    public async Task<Account> GetAsync(Username username, CancellationToken cancellationToken = default)
    {
        // 2) Monte a expressão para o EF (que compara duas strings):
        return await _readerAccountRepository
            .QuerySingleAsync<Account>(
                predicate: a => a.Username     == username,
                include: accounts => accounts.Include(a => a.Roles),
                cancellationToken: cancellationToken) ?? throw new KeyNotFoundException($"Account with username '{username.Value}' not found.");
    }

    public async Task<Account> GetAsync(Email email, CancellationToken cancellationToken = default)
    {
        // 2) Monte a expressão para o EF (que compara duas strings):
        return await _readerAccountRepository
            .QuerySingleAsync<Account>(
                predicate: a => a.Email     == email,
                include: accounts => accounts.Include(a => a.Roles),
                cancellationToken: cancellationToken) ?? throw new KeyNotFoundException($"Account with email '{email.Value}' not found.");
    }

    public async Task<Account> CreateAsync(Account account, CancellationToken cancellationToken = default)
    {
        account = await _writerAccountRepository.AddAsync(account, cancellationToken);
        
        return await _writerAccountRepository.SaveChangesAsync(cancellationToken)
            ? account
            : throw new InvalidOperationException("Failed to create account in the database.");
    }

    public async Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        // 2) Atualize a conta no repositório e salve as alterações:
        await _writerAccountRepository.UpdateAsync(account, cancellationToken);
        return await _writerAccountRepository.SaveChangesAsync(cancellationToken)
            ? account
            : throw new InvalidOperationException("Failed to update account in the database.");
    }
}
