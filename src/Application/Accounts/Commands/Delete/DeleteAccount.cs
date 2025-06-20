﻿using System.ComponentModel.DataAnnotations;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;
using GameServer.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Accounts.Commands.Delete;

public record DeleteAccountCommand(
    [Required] string userId
) : IRequest;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<DeleteAccountCommandHandler> _logger;

    public DeleteAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<DeleteAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }
        
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar entidade de domínio
            var entity = await _accountRepositoryReader.QuerySingleAsync(
                a => a.CreatedBy == request.userId,
                account => account,
                trackingType: TrackingType.Tracking,
                cancellationToken: cancellationToken
            );
            
            Guard.Against.Null(entity, nameof(entity), "Account not found");
            
            entity.Deactivate();

            // Deletar entidade
            await _accountRepositoryWriter.DeleteAsync(entity, cancellationToken);
            
            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);

            Guard.Against.Default(
                saveResult, 
                nameof(saveResult), 
                "Account could not be deleted from the database."
            );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain error while deleting account: {Message}", ex.Message);
            throw; // Re-throwing the exception to be handled by the caller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta");
            throw new ApplicationException("An error occurred while deleting the account.", ex);
        }
    }
}
