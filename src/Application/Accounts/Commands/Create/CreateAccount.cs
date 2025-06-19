using GameServer.Application.Accounts.Services;
using GameServer.Domain.Entities;
using GameServer.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Create;

public record CreateAccountCommand : IRequest;

public class CreateAccountCommandHandler(
    IAccountService accountService,
    ILogger<CreateAccountCommandHandler> logger)
    : IRequestHandler<CreateAccountCommand>
{
    public async Task Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Criar entidade de domínio
            var entity = Account.Create();

            // Salvar
            entity = await accountService.CreateAsync(entity, cancellationToken);
            
            logger.LogInformation("Conta criada com sucesso: {UserId}", entity.CreatedBy);
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
