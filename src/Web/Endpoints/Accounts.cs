using GameServer.Application.Accounts.Commands.Update;
using GameServer.Application.Accounts.Queries.GetAccount;
using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Common.Interfaces;
using GameServer.Domain.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GameServer.Web.Endpoints;

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
            var query = new GetAccountQuery();
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
        catch (DomainException ex)
        {
            return TypedResults.BadRequest(new[] { ex.Message });
        }
    }
}
