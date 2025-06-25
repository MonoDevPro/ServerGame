using GameServer.Application.Accounts.Commands.Example;
using GameServer.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GameServer.Web.Endpoints;

public class GameSessionExamples : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(RegularAction, "/regular-action")
            .MapGet(ReadOnlyData, "/readonly-data")
            .MapPost(VipAction, "/vip-action")
            .MapPost(StaffAction, "/staff-action")
            .MapGet(GameMasterReadOnly, "/gamemaster-readonly");
    }

    // Regular session required
    private async Task<Results<Ok, ProblemHttpResult>> RegularAction(
        ISender sender,
        IUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            return TypedResults.Problem("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);

        await sender.Send(new RegularGameActionCommand());
        return TypedResults.Ok();
    }

    // Allow expired session for read-only
    private async Task<Results<Ok<string>, ProblemHttpResult>> ReadOnlyData(
        ISender sender,
        IUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            return TypedResults.Problem("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);

        var result = await sender.Send(new ReadOnlyGameDataQuery());
        return TypedResults.Ok(result);
    }

    // VIP level required
    private async Task<Results<Ok, ProblemHttpResult>> VipAction(
        ISender sender,
        IUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            return TypedResults.Problem("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);

        await sender.Send(new VipOnlyActionCommand());
        return TypedResults.Ok();
    }

    // Staff level required
    private async Task<Results<Ok, ProblemHttpResult>> StaffAction(
        ISender sender,
        IUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            return TypedResults.Problem("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);

        await sender.Send(new StaffActionCommand());
        return TypedResults.Ok();
    }

    // GameMaster level required but allow expired session
    private async Task<Results<Ok<string>, ProblemHttpResult>> GameMasterReadOnly(
        ISender sender,
        IUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            return TypedResults.Problem("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);

        var result = await sender.Send(new GameMasterReadOnlyQuery());
        return TypedResults.Ok(result);
    }
}
