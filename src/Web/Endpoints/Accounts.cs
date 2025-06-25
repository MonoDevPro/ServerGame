using GameServer.Application.Accounts.Commands.Create;
using GameServer.Application.Accounts.Commands.Login;
using GameServer.Application.Accounts.Commands.Update;
using GameServer.Application.Accounts.Queries.Get;
using GameServer.Application.Accounts.Queries.Models;
using GameServer.Application.Common.Interfaces;
using GameServer.Domain.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Web.Endpoints;

public class Accounts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);
            
        group.RequireAuthorization()
            
            .MapGet(GetAccount)
            .MapPut(UpdateAccount, "") // Activity, Status, etc.
            .MapPost(LoginAccount, "/login") // novo endpoint
            .MapPost(LogoutAccount, "/logout"); // novo endpoint
    }

    private static async Task<IResult> GetAccount(IUser user, ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            var dto = await sender.Send(new GetAccountQuery());
            return Results.Ok(dto);
        }
        catch (NotFoundException)
        {
            return Results.NotFound();
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static readonly string[] Error = ["ID in URL must match ID in request body"];

    private static async Task<IResult> UpdateAccount(
        IUser user,
        [FromBody] AccountDto dto,
        ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            await sender.Send(new UpdateAccountCommand(dto));
            return Results.NoContent();
        }
        catch (NotFoundException)
        {
            return Results.NotFound();
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> LoginAccount(IUser user, ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        var loginResult = await sender.Send(new LoginAccountCommand());
        if (!loginResult.Success)
            return Results.BadRequest(new { error = "LOGIN_FAILED", message = loginResult.ErrorMessage });

        var dto = await sender.Send(new GetAccountQuery());
        return Results.Ok(dto);
    }

    private static async Task<IResult> LogoutAccount(IUser user, ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        await sender.Send(new LogoutAccountCommand());
        return Results.Ok();
    }
}
