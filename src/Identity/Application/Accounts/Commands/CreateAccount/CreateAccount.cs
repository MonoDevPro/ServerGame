using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Services;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Models;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Exceptions;
using ServerGame.Domain.ValueObjects;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Accounts.Commands.CreateAccount;

public record CreateAccountCommand(
    [Required] Username Username,
    [Required] Email Email
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
            var entity = Account.Create(request.Username, request.Email);

            // Salvar
            entity = await accountService.CreateAsync(entity, cancellationToken);
            
            logger.LogInformation("Conta criada com sucesso: {Username}, {Email}", entity.Username.Value, entity.Email.Value);
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
