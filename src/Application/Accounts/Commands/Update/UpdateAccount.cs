using System.ComponentModel.DataAnnotations;
using GameServer.Application.Accounts.Extensions;
using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;
using Microsoft.Extensions.Logging;
using ValidationException = FluentValidation.ValidationException;

namespace GameServer.Application.Accounts.Commands.Update;

public record UpdateAccountCommand(AccountDto AccountDto) : IRequest;

public class UpdateAccountCommandHandler(
    ILogger<UpdateAccountCommandHandler> logger)
    : IRequestHandler<UpdateAccountCommand>
{
    public Task Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Precisamos atualizar com handlers com objetivos específicos, como UpdateAccountNameCommand, UpdateAccountEmailCommand, etc.
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating account");
            throw;
        }
    }
}
