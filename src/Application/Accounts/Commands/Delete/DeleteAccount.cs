using GameServer.Application.Accounts.Services;
using GameServer.Application.Accounts.Services.Current;
using GameServer.Application.Common.Security;
using GameServer.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Delete;

[RequireGameSession]
public record DeleteAccountCommand : IRequest;

public class DeleteAccountCommandHandler(
    ICurrentAccountService currentAccountService
    ) : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        await currentAccountService.PurgeAsync(cancellationToken);
    }
}
