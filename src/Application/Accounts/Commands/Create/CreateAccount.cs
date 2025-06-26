using GameServer.Application.Accounts.Services;
using GameServer.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Create;

public record CreateAccountCommand : IRequest;

public class CreateAccountCommandHandler(
    ICurrentAccountService currentAccountService,
    ILogger<CreateAccountCommandHandler> logger)
    : IRequestHandler<CreateAccountCommand>
{
    /// <summary>
    /// Cria uma conta se ainda não existir. Idempotente.
    /// </summary>
    public async Task Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        if (await currentAccountService.ExistsAsync(cancellationToken))
            return;

        var account = await currentAccountService.EnsureCreatedAsync(cancellationToken);

        logger.LogInformation("Conta criada com sucesso: {UserId}", account.CreatedBy);
    }
}
