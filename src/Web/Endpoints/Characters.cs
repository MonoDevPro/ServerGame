using GameServer.Application.Characters.Commands.Create;
using GameServer.Application.Characters.Commands.Delete;
using GameServer.Application.Characters.Commands.Deselect;
using GameServer.Application.Characters.Commands.Select;
using GameServer.Application.Characters.Commands.Examples;
using GameServer.Application.Characters.Queries.Get;
using GameServer.Application.Characters.Queries.Models;
using GameServer.Application.Common.Interfaces;
using GameServer.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Web.Endpoints;

public class Characters : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);
            
        group.RequireAuthorization()
            
            .MapGet(GetAccountCharacters)
            .MapGet(GetCurrentCharacter, "/current")
            .MapPost(CreateCharacter, "")
            .MapPost(SelectCharacter, "/{id:long}/select")
            .MapPost(DeselectCharacter, "/deselect")
            .MapDelete(DeleteCharacter, "/{id:long}")
            
            // Exemplos de comandos específicos do jogo
            .MapPost(EnterDungeon, "/actions/enter-dungeon")
            .MapPost(ManageInventory, "/actions/manage-inventory")
            .MapPost(EnterPvpArena, "/actions/enter-pvp-arena");
    }

    private static async Task<IResult> GetAccountCharacters(IUser user, ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            var characters = await sender.Send(new GetAccountCharactersQuery());
            return Results.Ok(characters);
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

    private static async Task<IResult> GetCurrentCharacter(IUser user, ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            var character = await sender.Send(new GetCurrentCharacter());
            return Results.Ok(character);
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = "No character selected" });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CreateCharacter(
        IUser user,
        [FromBody] CreateCharacterRequest request,
        ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            var command = new CreateCharacterCommand(request.Name, request.Class);
            var result = await sender.Send(command);
            
            if (!result.Success)
                return Results.BadRequest(new { error = result.ErrorMessage });

            return Results.Created($"/characters/{result.CharacterId}", new { id = result.CharacterId });
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { 
                error = "Validation failed", 
                details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) 
            });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> SelectCharacter(
        IUser user,
        long id,
        ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            await sender.Send(new SelectCharacterCommand(id));
            return Results.Ok(new { message = "Character selected successfully" });
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = "Character not found" });
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { 
                error = "Validation failed", 
                details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) 
            });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeselectCharacter(
        IUser user,
        ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            await sender.Send(new DeselectCharacterCommand());
            return Results.Ok(new { message = "Character deselected successfully" });
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = "Character not found" });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteCharacter(
        IUser user,
        long id,
        ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            await sender.Send(new DeleteCharacterCommand(id));
            return Results.Ok(new { message = "Character deleted successfully" });
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = "Character not found" });
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { 
                error = "Validation failed", 
                details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) 
            });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    // Métodos para os exemplos de comandos do jogo
    private static async Task<IResult> EnterDungeon(IUser user, ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            await sender.Send(new EnterDungeonCommand());
            return Results.Ok(new { message = "Successfully entered dungeon", status = "in_dungeon" });
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = "Character not found or not selected" });
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { 
                error = "Validation failed", 
                details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) 
            });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ManageInventory(IUser user, ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            await sender.Send(new ManageInventoryCommand());
            return Results.Ok(new { message = "Inventory management session started" });
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = "Character not found or not selected" });
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { 
                error = "Validation failed", 
                details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) 
            });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> EnterPvpArena(IUser user, ISender sender)
    {
        if (string.IsNullOrEmpty(user.Id))
            return Results.Unauthorized();

        try
        {
            await sender.Send(new EnterPvpArenaCommand());
            return Results.Ok(new { message = "Successfully entered PVP arena", status = "in_pvp" });
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = "Character not found or not selected" });
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { 
                error = "Validation failed", 
                details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) 
            });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}

public record CreateCharacterRequest(string Name, GameServer.Domain.Enums.CharacterClass Class);
