using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Security;
using GameServer.Domain.Constants;
using GameServer.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Purge;

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
        // Deletar todas entidade
        await accountService.PurgeAsync(cancellationToken);
            
        logger.LogInformation("Accounts purged successfully");
    }
}
