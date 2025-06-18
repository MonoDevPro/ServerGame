using Microsoft.AspNetCore.Http.HttpResults;
using ServerGame.Api.Infrastructure;
using ServerGame.Application.Accounts.Commands.Create;
using ServerGame.Application.Accounts.Commands.Delete;
using ServerGame.Application.Accounts.Commands.Purge;
using ServerGame.Application.Accounts.Commands.Update;
using ServerGame.Application.Accounts.Models;
using ServerGame.Application.Accounts.Queries.GetAccount;
using ServerGame.Application.Common.Exceptions;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Domain.Exceptions;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Api.Endpoints;

public class Accounts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(Get)
            .MapPut(Update, "{id}");
    }

    private async Task<Results<Ok<AccountDto>, NotFound>> Get(
        ISender sender,
        IUser user)   // ← aqui
    {
        try
        {
            var query = new GetAccountInfoQuery();
            var result = await sender.Send(query);
            return TypedResults.Ok(result);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (DomainException)
        {
            return TypedResults.NotFound();
        }
    }

    private static readonly string[] Error = ["ID in URL must match ID in request body"];

    private async Task<Results<Ok, BadRequest<string[]>, NotFound>> Update(ISender sender, IUser user)
    {
        var id = user.Id;

        if (string.IsNullOrEmpty(id))
        {
            return TypedResults.BadRequest(Error);
        }
        
        var command = new UpdateAccountCommand(id);

        try
        {
            await sender.Send(command);
            return TypedResults.Ok();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(new[] { ex.Message });
        }
        catch (DomainException ex)
        {
            return TypedResults.BadRequest(new[] { ex.Message });
        }
    }
}
