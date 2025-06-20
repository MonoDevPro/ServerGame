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
    IAccountService accountService,
    ILogger<UpdateAccountCommandHandler> logger)
    : IRequestHandler<UpdateAccountCommand>
{
    public async Task Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Retrieve the existing account
            var existingAccount = await accountService.GetAsync(cancellationToken);
            if (existingAccount == null)
                throw new ValidationException($"Account with User does not exist.");

            // Update the account properties
            existingAccount = existingAccount.Update(request.AccountDto);

            // Save the updated account
            await accountService.UpdateAsync(existingAccount, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating account");
            throw;
        }
    }
}
