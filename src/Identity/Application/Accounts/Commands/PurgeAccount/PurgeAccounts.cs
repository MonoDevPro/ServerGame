using System.ComponentModel.DataAnnotations;
using GameServer.Shared.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Commands.CreateAccount;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Security;
using ServerGame.Domain.Constants;
using ServerGame.Domain.Entities;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.Accounts.Commands.PurgeAccount;

[Authorize(Roles = Roles.Administrator)]
[Authorize(Policy = Policies.CanPurge)]
public record PurgeAccountCommand : IRequest;

public class DeleteAccountCommandHandler : IRequestHandler<PurgeAccountCommand>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public DeleteAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<CreateAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }
        
    public async Task Handle(PurgeAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar entidades de domínio
            var entities = await _accountRepositoryReader.QueryListAsync<Account>(
                cancellationToken: cancellationToken
            );

            // Deletar todas entidade
            await _accountRepositoryWriter.DeleteRangeAsync(entities, cancellationToken);
            
            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);

            Guard.Against.Default(
                saveResult, 
                nameof(saveResult), 
                "Account could not be deleted from the database."
            );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain error while deleting account: {Message}", ex.Message);
            throw; // Re-throwing the exception to be handled by the caller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta");
            throw new ApplicationException("An error occurred while deleting the account.", ex);
        }
    }
}
