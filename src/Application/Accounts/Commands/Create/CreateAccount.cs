using GameServer.Application.Accounts.Services;
using GameServer.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Create;

public record CreateAccountCommand : IRequest;

public class CreateAccountCommandHandler(
    IAccountService accountService,
    ILogger<CreateAccountCommandHandler> logger)
    : IRequestHandler<CreateAccountCommand>
{
    /// <summary>
    /// Cria uma conta se ainda não existir. Idempotente.
    /// </summary>
    public async Task Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await accountService.ExistsAsync(cancellationToken))
                return;

            var account = await accountService.CreateAsync(cancellationToken);

            logger.LogInformation("Conta criada com sucesso: {UserId}", account.CreatedBy);
        }
        catch (DomainException ex)
        {
            logger.LogError(ex, "Erro de domínio ao criar conta: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao criar conta");
        }
    }
}
