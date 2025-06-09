using Microsoft.AspNetCore.Http.HttpResults;
using ServerGame.Api.Infrastructure;
using ServerGame.Application.Accounts.Commands.CreateAccount;
using ServerGame.Application.Accounts.Commands.DeleteAccount;
using ServerGame.Application.Accounts.Commands.PurgeAccount;
using ServerGame.Application.Accounts.Commands.UpdateAccount;
using ServerGame.Application.Accounts.Models;
using ServerGame.Application.Accounts.Queries.GetSingleAccount;
using ServerGame.Application.Common.Exceptions;
using ServerGame.Domain.Exceptions;
using ServerGame.Domain.ValueObjects;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Api.Endpoints;

public class Accounts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(Get, "{usernameOrEmail}")
            .MapPost(Create)
            .MapPut(Update, "{id}")
            .MapDelete(Delete, "{usernameOrEmail}")
            .MapDelete(Purge, "purge");
    }

    private async Task<Results<Ok<AccountDto>, NotFound>> Get(ISender sender, string usernameOrEmail)
    {
        if (!UsernameOrEmail.TryCreate(usernameOrEmail, out var usernameOrEmailVO))
            return TypedResults.NotFound();

        try
        {
            var query = new GetSingleAccountQuery(usernameOrEmailVO!);
            var result = await sender.Send(query);
            return TypedResults.Ok(result);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private async Task<Results<Created<string>, BadRequest<string[]>>> Create(ISender sender, CreateAccountCommand command)
    {
        try
        {
            await sender.Send(command);
            return TypedResults.Created($"/{nameof(Accounts)}/{command.Username.Value}", command.Username.Value);
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

    private async Task<Results<NoContent, BadRequest<string[]>, NotFound>> Update(ISender sender, long id, UpdateAccountCommand command)
    {
        if (id != command.Id) 
            return TypedResults.BadRequest(new[] { "ID in URL must match ID in request body" });

        try
        {
            await sender.Send(command);
            return TypedResults.NoContent();
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

    private async Task<Results<NoContent, BadRequest<string[]>, NotFound>> Delete(ISender sender, string usernameOrEmail)
    {
        if (!UsernameOrEmail.TryCreate(usernameOrEmail, out var usernameOrEmailVO))
        {
            return TypedResults.BadRequest(new[] { "Invalid username or email format" });
        }

        try
        {
            var command = new DeleteAccountCommand(usernameOrEmailVO!);
            await sender.Send(command);
            return TypedResults.NoContent();
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

    private async Task<Results<NoContent, BadRequest<string[]>, ForbidHttpResult>> Purge(ISender sender)
    {
        try
        {
            var command = new PurgeAccountCommand();
            await sender.Send(command);
            return TypedResults.NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
        catch (ForbiddenAccessException)
        {
            return TypedResults.Forbid();
        }
        catch (DomainException ex)
        {
            return TypedResults.BadRequest(new[] { ex.Message });
        }
    }
}
