using System.ComponentModel.DataAnnotations;
using GameServer.Shared.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Security;
using ServerGame.Domain.Constants;
using ServerGame.Domain.Entities;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.Accounts.Commands.DeactivateAccount;

[Authorize(Roles = Roles.Administrator)]
public record DeactivateAccountCommand(
    [Required] UsernameOrEmail UsernameOrEmail
) : IRequest;

public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<DeactivateAccountCommandHandler> _logger;

    public DeactivateAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<DeactivateAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }

    public async Task Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar entidade de domínio
            var entity = await _accountRepositoryReader.QuerySingleAsync(
                a => a.Email.Value == request.UsernameOrEmail.Value 
                     || a.Username.Value == request.UsernameOrEmail.Value,
                account => account,
                trackingType: TrackingType.Tracking,
                cancellationToken: cancellationToken
            );
            
            Guard.Against.Null(entity, nameof(entity), "Account not found");
            
            // Desativar conta
            entity.Deactivate();

            // Salvar
            await _accountRepositoryWriter.UpdateAsync(entity, cancellationToken);
            
            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);

            Guard.Against.Default(
                saveResult, 
                nameof(saveResult), 
                "Account could not be deactivated in the database."
            );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain error while deactivating account: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deactivating account");
            throw new ApplicationException("An error occurred while deactivating the account.", ex);
        }
    }
}
