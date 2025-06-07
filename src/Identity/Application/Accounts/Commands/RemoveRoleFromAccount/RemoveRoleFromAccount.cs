using System.ComponentModel.DataAnnotations;
using GameServer.Shared.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Security;
using ServerGame.Domain.Constants;
using ServerGame.Domain.Entities;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.Accounts.Commands.RemoveRoleFromAccount;

[Authorize(Roles = Roles.Administrator)]
public record RemoveRoleFromAccountCommand(
    [Required] UsernameOrEmail UsernameOrEmail,
    [Required] string RoleName
) : IRequest;

public class RemoveRoleFromAccountCommandHandler : IRequestHandler<RemoveRoleFromAccountCommand>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<RemoveRoleFromAccountCommandHandler> _logger;

    public RemoveRoleFromAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<RemoveRoleFromAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }

    public async Task Handle(RemoveRoleFromAccountCommand request, CancellationToken cancellationToken)
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

            if (role is null)
            {
                throw new ArgumentException($"Invalid role: '{request.RoleName}'", nameof(request.RoleName));
            }
            
            // Remover role
            entity.RemoveRole(role);

            // Salvar
            await _accountRepositoryWriter.UpdateAsync(entity, cancellationToken);
            
            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);

            Guard.Against.Default(
                saveResult, 
                nameof(saveResult), 
                "Role could not be removed from account in the database."
            );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain error while removing role from account: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while removing role from account");
            throw new ApplicationException("An error occurred while removing role from the account.", ex);
        }
    }
}
