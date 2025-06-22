using System.ComponentModel.DataAnnotations;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;
using GameServer.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Delete;

public record DeleteAccountCommand : IRequest;

public class DeleteAccountCommandHandler(
    IAccountService accountService,
    ILogger<DeleteAccountCommandHandler> logger) : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await accountService.GetForUpdateAsync(cancellationToken);
            entity.Deactivate();
        }
        catch (DomainException ex)
        {
            logger.LogError(ex, "Command error while deleting account: {Message}", ex.Message);
            throw; // Re-throwing the exception to be handled by the caller
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Command error while deleting account: {Message}", ex.Message);
            throw new ApplicationException("An error occurred while deleting the account.", ex);
        }
    }
}
