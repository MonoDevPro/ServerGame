using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;

namespace GameServer.Infrastructure.Persistence;

public class AccountService(
    IRepositoryCompose<Account> accountRepository)
    : IAccountService
{
    private readonly IRepositoryCompose<Account> _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    private IReaderRepository<Account> ReaderAccountRepository => _accountRepository.ReaderRepository;
    private IWriterRepository<Account> WriterAccountRepository => _accountRepository.WriterRepository;

    public async Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default)
    {
        // 2) Execute a consulta e retorne o resultado:
        return await ReaderAccountRepository
            .ExistsAsync(a => a.CreatedBy == userId,
                cancellationToken: cancellationToken);
    }

    public async Task<Account> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        // 2) Monte a expressão para o EF (que compara duas strings):
        return await ReaderAccountRepository
            .QuerySingleAsync<Account>(
                predicate: a => a.CreatedBy == userId,
                trackingType: TrackingType.NoTracking,
                cancellationToken: cancellationToken) ?? throw new KeyNotFoundException($"Account with userId '{userId}' not found.");
    }

    public async Task<Account> CreateAsync(Account account, CancellationToken cancellationToken = default)
    {
        account = await WriterAccountRepository.AddAsync(account, cancellationToken);
        
        return await WriterAccountRepository.SaveChangesAsync(cancellationToken)
            ? account
            : throw new InvalidOperationException("Failed to create account in the database.");
    }

    public async Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        // 2) Atualize a conta no repositório e salve as alterações:
        await WriterAccountRepository.UpdateAsync(account, cancellationToken);
        return await WriterAccountRepository.SaveChangesAsync(cancellationToken)
            ? account
            : throw new InvalidOperationException("Failed to update account in the database.");
    }

    public async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await ReaderAccountRepository
                .QueryListAsync<Account>(
                    trackingType: TrackingType.Tracking,
                    cancellationToken: cancellationToken);
            
            if (accounts.Count == 0)
                throw new InvalidOperationException("No accounts found to purge.");
            
            foreach (var account in accounts)
            {
                // 1) Remove all roles from the account:
                account.Deactivate();
                
                // 2) Remove the account from the repository:
                await WriterAccountRepository.UpdateAsync(account, cancellationToken);
            }
            
            // 3) Save changes to the database:
            var saveResult = await WriterAccountRepository.SaveChangesAsync(cancellationToken);

            if (!saveResult)
            {
                throw new InvalidOperationException("Failed to delete all accounts from the database.");
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            throw new InvalidOperationException("An error occurred while purging accounts.", ex);
        }
    }
}
