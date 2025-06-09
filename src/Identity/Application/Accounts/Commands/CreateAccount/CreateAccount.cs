using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
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

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand>
{
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public CreateAccountCommandHandler(
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<CreateAccountCommandHandler> logger
    )
    {
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }
    
    public async Task Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Criar entidade de domínio
            var entity = new Account(request.Username, request.Email);

            // Salvar
            await _accountRepositoryWriter.AddAsync(entity, cancellationToken);

            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);
            
            Guard.Against.Default(
                saveResult, 
                nameof(saveResult), 
                "Account could not be created in the database."
            );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Erro de domínio ao criar conta: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta");
        }
    }
}
