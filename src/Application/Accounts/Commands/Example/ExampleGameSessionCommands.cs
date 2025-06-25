using GameServer.Application.Common.Security;

namespace GameServer.Application.Accounts.Commands.Example;

// Example 1: Regular session required
[RequireGameSession]
public record RegularGameActionCommand : IRequest<Unit>;

// Example 2: Allow expired session for read-only operations
[RequireGameSession(AllowExpiredSession = true)]
public record ReadOnlyGameDataQuery : IRequest<string>;

// Example 3: Require VIP level (1) or higher
[RequireGameSession(MinimumLevel = 1)]
public record VipOnlyActionCommand : IRequest<Unit>;

// Example 4: Require Staff level (3) or higher
[RequireGameSession(MinimumLevel = 3)]
public record StaffActionCommand : IRequest<Unit>;

// Example 5: Allow expired session but require minimum level
[RequireGameSession(AllowExpiredSession = true, MinimumLevel = 2)]
public record GameMasterReadOnlyQuery : IRequest<string>;

public class ExampleCommandHandlers :
    IRequestHandler<RegularGameActionCommand, Unit>,
    IRequestHandler<ReadOnlyGameDataQuery, string>,
    IRequestHandler<VipOnlyActionCommand, Unit>,
    IRequestHandler<StaffActionCommand, Unit>,
    IRequestHandler<GameMasterReadOnlyQuery, string>
{
    public Task<Unit> Handle(RegularGameActionCommand request, CancellationToken cancellationToken)
    {
        // This requires an active, valid game session
        return Task.FromResult(Unit.Value);
    }

    public Task<string> Handle(ReadOnlyGameDataQuery request, CancellationToken cancellationToken)
    {
        // This allows reading even if session is expired
        return Task.FromResult("Game data");
    }

    public Task<Unit> Handle(VipOnlyActionCommand request, CancellationToken cancellationToken)
    {
        // This requires AccountType.VIP (1) or higher
        return Task.FromResult(Unit.Value);
    }

    public Task<Unit> Handle(StaffActionCommand request, CancellationToken cancellationToken)
    {
        // This requires AccountType.Staff (3) or higher
        return Task.FromResult(Unit.Value);
    }

    public Task<string> Handle(GameMasterReadOnlyQuery request, CancellationToken cancellationToken)
    {
        // This allows expired session but requires AccountType.GameMaster (2) or higher
        return Task.FromResult("Staff data");
    }
}
