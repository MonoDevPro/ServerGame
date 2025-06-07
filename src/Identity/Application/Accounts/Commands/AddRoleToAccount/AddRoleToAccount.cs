using System.ComponentModel.DataAnnotations;
using GameServer.Shared.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Security;
using ServerGame.Domain.Constants;
using ServerGame.Domain.Entities;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.Accounts.Commands.AddRoleToAccount;

[Authorize(Roles = Roles.Administrator)]
public record AddRoleToAccountCommand(
    [Required] UsernameOrEmail UsernameOrEmail,
    [Required] string RoleName
) : IRequest;

public class AddRoleToAccountCommandHandler : IRequestHandler<AddRoleToAccountCommand>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<AddRoleToAccountCommandHandler> _logger;

    public AddRoleToAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<AddRoleToAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }

    public async Task Handle(AddRoleToAccountCommand request, CancellationToken cancellationToken)
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
            
            // Criar role
            var role = Role.Create(request.RoleName);
            
            Guard.Against.Null(role, nameof(role), "Role could not be created");
            
            // Adicionar role
            entity.AddRole(role);

            // Salvar
            await _accountRepositoryWriter.UpdateAsync(entity, cancellationToken);
            
            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);

            Guard.Against.Default(
                saveResult, 
                nameof(saveResult), 
                "Role could not be added to account in the database."
            );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain error while adding role to account: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding role to account");
            throw new ApplicationException("An error occurred while adding role to the account.", ex);
        }
    }
}
