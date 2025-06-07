using System.ComponentModel.DataAnnotations;
using GameServer.Shared.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Security;
using ServerGame.Domain.Constants;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Enums;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.Accounts.Commands.PromoteAccount;

[Authorize(Roles = Roles.Administrator)]
public record PromoteAccountCommand(
    [Required] UsernameOrEmail UsernameOrEmail,
    [Required] AccountType NewAccountType
) : IRequest;

public class PromoteAccountCommandHandler : IRequestHandler<PromoteAccountCommand>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<PromoteAccountCommandHandler> _logger;

    public PromoteAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<PromoteAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }

    public async Task Handle(PromoteAccountCommand request, CancellationToken cancellationToken)
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
            
            // Promover conta
            entity.PromoteTo(request.NewAccountType);

            // Salvar
            await _accountRepositoryWriter.UpdateAsync(entity, cancellationToken);
            
            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);

            Guard.Against.Default(
                saveResult, 
                nameof(saveResult), 
                "Account could not be promoted in the database."
            );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain error while promoting account: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while promoting account");
            throw new ApplicationException("An error occurred while promoting the account.", ex);
        }
    }
}
