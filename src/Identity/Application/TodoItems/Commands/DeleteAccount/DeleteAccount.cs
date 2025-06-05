using System.ComponentModel.DataAnnotations;
using GameServer.Shared.Database.Repository.Reader;
using GameServer.Shared.Database.Repository.UnityOfWork;
using GameServer.Shared.Database.Repository.Writer;
using GameServer.Shared.Domain.Exceptions;
using GameServer.Shared.EventBus.EventBus;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Models;
using ServerGame.Application.TodoItems.Commands.CreateAccount;
using ServerGame.Domain.Entities;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.TodoItems.Commands.DeleteAccount;

public record DeleteAccountCommand(
    [Required] string Username,
    [Required] string Email
) : IRequest<Result>;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Result>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public DeleteAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<CreateAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }
        
    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Lógica de validação
            // Criar objetos de valor
            var username = UsernameVO.Create(request.Username);
            var email = EmailVO.Create(request.Email);

            var existingAccount = await _accountRepositoryReader.ExistsAsync(
                a => a.Email.Value == email.Value || a.Username.Value == username.Value, cancellationToken);

            if (!existingAccount)
                return Result.Failure(["Username or email not exists"]);

            // Buscar entidade de domínio
            var entity = await _accountRepositoryReader.QuerySingleAsync(
                a => a.Email.Value == email.Value && a.Username.Value == username.Value,
                account => account,
                trackingType: TrackingType.Tracking,
                cancellationToken: cancellationToken
            );
            
            if (entity == null)
                return Result.Failure(["Account not found"]);
            
            entity.Deactivate();

            // Deletar entidade
            await _accountRepositoryWriter.DeleteAsync(entity, cancellationToken);
            
            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);

            if (!saveResult)
                return Result.Failure(["Error to delete account in database"]);

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
