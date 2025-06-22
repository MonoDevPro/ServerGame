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
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(Get)
            .MapPut(Update, "") // Activity, Status, etc.
            .MapPost(Login, "/login"); // novo endpoint
            .MapPost(Logout, "/logout") // novo endpoint
    }

    private async Task<Results<Ok<AccountDto>, NotFound, ProblemHttpResult>> Get(
        ISender sender,
        IUser user)
    {
        try
        {
            if (string.IsNullOrEmpty(user.Id))
                return TypedResults.NotFound();

            var query = new GetAccountQuery();
            var result = await sender.Send(query);
            return TypedResults.Ok(result);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (DomainException ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: $"Erro ao obter dados da conta: {ex.Message}",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static readonly string[] Error = ["ID in URL must match ID in request body"];

    private async Task<Results<Ok, BadRequest<string[]>, NotFound>> Update(
        [FromBody] AccountDto accountDto,
        ISender sender,
        IUser user)
    {
        // Verifica se o usuário está autenticado
        if (string.IsNullOrEmpty(user.Id))
            return TypedResults.BadRequest(Error);

        var command = new UpdateAccountCommand(accountDto);

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

    // Novo: POST /login
    private async Task<Results<Ok<AccountDto>, ProblemHttpResult>> Login(
        ISender sender,
        IUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            return TypedResults.Problem(
                detail: "Usuário não autenticado.",
                statusCode: StatusCodes.Status401Unauthorized);

        // Dispara o comando de login (que criará a conta se não existir e registrará o login)
        await sender.Send(new LoginAccountCommand());

        // Recupera a conta já atualizada
        var account = await sender.Send(new GetAccountQuery());

        return TypedResults.Ok(account);
    }

    // Noov: POST /create
    private async Task<Results<Ok<AccountDto>, ProblemHttpResult>> Create(
        ISender sender,
        IUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            return TypedResults.Problem(
                detail: "Usuário não autenticado.",
                statusCode: StatusCodes.Status401Unauthorized);
        // Dispara o comando de criação de conta
        await sender.Send(new CreateAccountCommand());
        var account = await sender.Send(new GetAccountQuery());
        return TypedResults.Ok(account);
    }

    // Novo: POST /logout
    private async Task<Results<Ok, ProblemHttpResult>> Logout(
        ISender sender,
        IUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            return TypedResults.Problem(
                detail: "Usuário não autenticado.",
                statusCode: StatusCodes.Status401Unauthorized);

        // Dispara o comando de logout
        await sender.Send(new LogoutAccountCommand());
        return TypedResults.Ok();
    }
}