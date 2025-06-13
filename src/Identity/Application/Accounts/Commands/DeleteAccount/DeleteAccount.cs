using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Commands.CreateAccount;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Models;
using ServerGame.Application.Common.Security;
using ServerGame.Domain.Constants;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Exceptions;
using ServerGame.Domain.ValueObjects;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Accounts.Commands.DeleteAccount;

public record DeleteAccountCommand(
    [Required] UsernameOrEmail UsernameOrEmail
) : IRequest;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<DeleteAccountCommandHandler> _logger;

    public DeleteAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<DeleteAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }
        
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar entidade de domínio
            var entity = await _accountRepositoryReader.QuerySingleAsync(
                a => a.Email == request.UsernameOrEmail 
                     || a.Username == request.UsernameOrEmail,
                account => account,
                trackingType: TrackingType.Tracking,
                cancellationToken: cancellationToken
            );
            
            Guard.Against.Null(entity, nameof(entity), "Account not found");
            
            entity.Deactivate();

            // Deletar entidade
            await _accountRepositoryWriter.DeleteAsync(entity, cancellationToken);
            
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
