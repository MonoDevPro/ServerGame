using System.ComponentModel.DataAnnotations;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Update;

public record UpdateAccountCommand(
    [Required] string UserId
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
                a => a.CreatedBy == request.UserId,
                trackingType: TrackingType.Tracking,
                cancellationToken: cancellationToken
            );
            
            Guard.Against.Null(
                entity, 
                nameof(entity), 
                "Account not found in the database."
            );

            // Salvar
            await _accountRepositoryWriter.UpdateAsync(entity, cancellationToken);

            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);
            
            Guard.Against.Default(
                saveResult, 
                nameof(saveResult), 
                "Account could not be updated in the database."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating account");
            throw;
        }
    }
}
