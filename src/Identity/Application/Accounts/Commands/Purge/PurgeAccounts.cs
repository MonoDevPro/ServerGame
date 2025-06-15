using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Services;
using ServerGame.Application.Common.Security;
using ServerGame.Domain.Constants;
using ServerGame.Domain.Exceptions;

namespace ServerGame.Application.Accounts.Commands.Purge;

[Authorize(Roles = Roles.Administrator)]
[Authorize(Policy = Policies.CanPurge)]
public record PurgeAccountCommand : IRequest;

public class DeleteAccountCommandHandler(
    IAccountService accountService,
    ILogger<DeleteAccountCommandHandler> logger)
    : IRequestHandler<PurgeAccountCommand>
{
    public async Task Handle(PurgeAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Deletar todas entidade
            await accountService.PurgeAsync(cancellationToken);
            
            logger.LogInformation("Accounts purged successfully");
        }
        catch (DomainException ex)
        {
            logger.LogError(ex, "Domain error while deleting accounts: {Message}", ex.Message);
            throw; // Re-throwing the exception to be handled by the caller
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting accounts: {Message}", ex.Message);
            throw new ApplicationException("An error occurred while deleting the accounts.", ex);
        }
    }
}
