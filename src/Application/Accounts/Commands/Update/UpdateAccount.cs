using System.ComponentModel.DataAnnotations;
using GameServer.Application.Accounts.Extensions;
using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Accounts.Services;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Application.Common.Security;
using GameServer.Domain.Entities;
using Microsoft.Extensions.Logging;
using ValidationException = FluentValidation.ValidationException;

namespace GameServer.Application.Accounts.Commands.Update;

[RequireGameSession]
public record UpdateAccountCommand(AccountDto AccountDto) : IRequest;

public class UpdateAccountCommandHandler
    : IRequestHandler<UpdateAccountCommand>
{
    public Task Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        // TODO: Precisamos atualizar com handlers com objetivos específicos, como UpdateAccountNameCommand, UpdateAccountEmailCommand, etc.
        return Task.CompletedTask;
    }
}
