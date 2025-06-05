using System.ComponentModel.DataAnnotations;
using GameServer.Shared.Database.Repository.Reader;
using GameServer.Shared.Database.Repository.UnityOfWork;
using GameServer.Shared.Database.Repository.Writer;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Models;
using ServerGame.Domain.Entities;
using ServerGame.Domain.ValueObjects;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace ServerGame.Application.TodoItems.Commands.UpdateAccount;

public record UpdateAccountCommand(
    [Required] string Username,
    [Required] string Email
) : IRequest<Result>;

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, Result>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<UpdateAccountCommandHandler> _logger;

    public UpdateAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<UpdateAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
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

            // Atualizar entidade de domínio
            entity.UpdateUsername(username);
            entity.UpdateEmail(email);

            // Salvar
            await _accountRepositoryWriter.UpdateAsync(entity, cancellationToken);

            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);

            if (!saveResult)
                return Result.Failure(["Error to update account in database"]);

            return Result.Success();
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred while updating account");
            return Result.Failure([ex.Message]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating account");
            return Result.Failure(["An unexpected error occurred"]);
        }
    }
}
