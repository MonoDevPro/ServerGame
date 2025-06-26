using GameServer.Application.Accounts.Services;
using GameServer.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Delete;

public record DeleteAccountCommand : IRequest;

public class DeleteAccountCommandHandler(
    ICurrentAccountService currentAccountService
    ) : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
            var entity = await currentAccountService.GetForUpdateAsync(cancellationToken);
            entity.Deactivate();
    }
}
