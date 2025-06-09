using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Models;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects;
using ServerGame.Domain.ValueObjects.Accounts;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace ServerGame.Application.Accounts.Commands.UpdateAccount;

public record UpdateAccountCommand(
    [Required] long Id,
    [Required] Username Username,
    [Required] Email Email
) : IRequest;

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<UpdateAccountCommandHandler> _logger;

    public UpdateAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<UpdateAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }

    public async Task Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar entidade de domínio
            var entity = await _accountRepositoryReader.QuerySingleAsync<Account>(
                a => a.Id == request.Id,
                trackingType: TrackingType.NoTracking,
                cancellationToken: cancellationToken
            );
            
            Guard.Against.Null(
                entity, 
                nameof(entity), 
                "Account not found in the database."
            );

            // Atualizar entidade de domínio
            entity.UpdateUsername(request.Username);
            entity.UpdateEmail(request.Email);

            // Salvar
            await _accountRepositoryWriter.UpdateAsync(entity, cancellationToken);

            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);
            
            Guard.Against.Default(
                saveResult, 
                nameof(saveResult), 
                "Account could not be updated in the database."
            );
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred while updating account");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating account");
            throw;
        }
    }
}
