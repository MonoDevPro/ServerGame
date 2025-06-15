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
            .MapPost(Create) // ← descomente se quiser permitir criação de contas via API
            .MapPut(Update, "{id}")
            .MapDelete(Delete, "{usernameOrEmail}")
            .MapDelete(Purge, "purge");
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

    private static readonly string[] Error = ["ID in URL must match ID in request body"];
    private static readonly string[] error = new[] { "Invalid username or email format" };

    private async Task<Results<Ok, BadRequest<string[]>, NotFound>> Update(ISender sender, long id, UpdateAccountCommand command)
    {
        if (id != command.Id) 
            return TypedResults.BadRequest(Error);

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

    private async Task<Results<Ok, BadRequest<string[]>, NotFound>> Delete(ISender sender, string usernameOrEmail)
    {
        if (!UsernameOrEmail.TryCreate(usernameOrEmail, out var usernameOrEmailVO))
            return TypedResults.BadRequest(error);

        try
        {
            var command = new DeleteAccountCommand(usernameOrEmailVO!);
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

    private async Task<Results<Ok, BadRequest<string[]>, ForbidHttpResult>> Purge(ISender sender)
    {
        try
        {
            var commandAccount = new PurgeAccountCommand();
            await sender.Send(commandAccount);
            
            return TypedResults.Ok();
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
