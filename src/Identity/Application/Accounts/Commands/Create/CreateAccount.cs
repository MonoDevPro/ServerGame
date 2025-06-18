using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Services;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Exceptions;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Accounts.Commands.Create;

public record CreateAccountCommand(
    [Required] string userId
) : IRequest;

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
