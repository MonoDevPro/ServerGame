using System.ComponentModel.DataAnnotations;
using GameServer.Shared.Database.Repository.Reader;
using GameServer.Shared.Database.Repository.Writer;
using GameServer.Shared.Domain.Exceptions;
using GameServer.Shared.EventBus.EventBus;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Models;
using ServerGame.Domain.Entities;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.TodoItems.Commands.CreateAccount;

public record CreateAccountCommand(
    [Required] string Username,
    [Required] string Email
) : IRequest<Result>;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public CreateAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<CreateAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }
        
    public async Task<Result> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Lógica de validação
            // Criar objetos de valor
            var username = UsernameVO.Create(request.Username);
            var email = EmailVO.Create(request.Email);

            var existingAccount = await _accountRepositoryReader.ExistsAsync(
                a => a.Email == email || a.Username == username, cancellationToken);

            if (existingAccount)
                return Result.Failure(["Username or email already exists"]);

            // Criar entidade de domínio
            var entity = new Account(username, email);

            // Salvar
            var entityToPersist = await _accountRepositoryWriter.AddAsync(entity, cancellationToken);

            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);

            if (!saveResult)
                return Result.Failure(["Error to save account in database"]);
                
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure([ex.Message]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta");
            return Result.Failure(["Erro interno ao processar o registro"]);
        }
    }
}
