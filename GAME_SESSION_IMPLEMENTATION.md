# Game Session Management Implementation

This implementation consolidates game session enforcement via MediatR PipelineBehavior, using sliding-only expiration and comprehensive metrics.

## Features Implemented

### 1. **Sliding-Only Expiration**
- Removed `AbsoluteExpirationRelativeToNow` from cache configuration
- Sessions now use only `SlidingExpiration` (default: 30 minutes)
- Sessions automatically extend on each valid request

### 2. **Unified Enforcement via PipelineBehavior**
- All session validation happens in `GameSessionBehavior<TRequest, TResponse>`
- No middleware or controller filters needed for session validation
- Works with `[RequireGameSession]` attribute on commands/queries

### 3. **Enhanced RequireGameSessionAttribute**
```csharp
[RequireGameSession(AllowExpiredSession = false, MinimumLevel = 0)]
public record MyCommand : IRequest;
```

**Properties:**
- `AllowExpiredSession`: Allow read operations even if session expired
- `MinimumLevel`: Minimum AccountType level required (Player=0, VIP=1, GameMaster=2, Staff=3, Administrator=4)

### 4. **Session TTL Communication**
- Middleware `GameSessionTtlMiddleware` adds `X-Game-Session-Expires-In` header
- Header contains seconds until session expires
- Added after successful requests with valid sessions

### 5. **Comprehensive Metrics**
Using `System.Diagnostics.Metrics`:
- `game_sessions_created`: Count of new sessions
- `game_sessions_renewed`: Count of session renewals
- `game_sessions_expired`: Count of expired sessions
- `game_sessions_revoked`: Count of manually revoked sessions

## Usage Examples

### Basic Session Requirement
```csharp
[RequireGameSession]
public record PlayGameCommand : IRequest;
```

### Allow Expired Session (Read-Only)
```csharp
[RequireGameSession(AllowExpiredSession = true)]
public record GetPlayerStatsQuery : IRequest<PlayerStats>;
```

### Minimum Level Requirement
```csharp
// Requires VIP level or higher
[RequireGameSession(MinimumLevel = 1)]
public record VipFeatureCommand : IRequest;

// Requires Staff level or higher
[RequireGameSession(MinimumLevel = 3)]
public record ModeratePlayerCommand : IRequest;
```

### Combined Settings
```csharp
// Allow expired session but require GameMaster level
[RequireGameSession(AllowExpiredSession = true, MinimumLevel = 2)]
public record ViewModerationLogsQuery : IRequest<ModerationLog[]>;
```

## Configuration

### Program.cs
```csharp
// Middleware for TTL headers
app.UseMiddleware<GameSessionTtlMiddleware>();
```

### Pipeline Behavior Registration
The `GameSessionBehavior` is automatically registered in `Application.DependencyInjection.cs`:

```csharp
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(GameSessionBehavior<,>));
```

## Session Management Methods

### IGameSessionService New Methods
- `IsSessionValidAsync(userId)`: Check if session is active (not expired)
- `GetProfileAsync(userId)`: Get user profile for level verification
- `GetSessionTtlAsync(userId)`: Get remaining session time
- `RefreshSessionAsync(userId)`: Manually refresh session

## Error Handling

### Exception Types
- `GameSessionRequiredException`: Session required but not found/expired
- `ForbiddenException`: Insufficient level or other authorization failure
- `UnauthorizedAccessException`: User not authenticated

### HTTP Status Codes
- 401: User not authenticated
- 403: Insufficient privileges (level too low)
- 400: Session required but not found

## Metrics Integration

The service automatically tracks session lifecycle events. To view metrics:

1. Add metrics endpoint in your application
2. Use OpenTelemetry or similar metrics collector
3. Monitor the `GameServer.GameSession` meter

## Level System

The implementation uses `AccountType` enum as level system:
- Player = 0 (default)
- VIP = 1
- GameMaster = 2
- Staff = 3
- Administrator = 4

Higher values have access to lower-level requirements.

## Migration Notes

### Removed Components
- Any HTTP authorization middleware for game sessions
- Controller-level session filters
- Absolute expiration configuration

### Updated Components
- `GameSessionService`: Enhanced with metrics and new methods
- `GameSessionBehavior`: Complete rewrite with level checking
- `RequireGameSessionAttribute`: Enhanced with new properties

This implementation provides a clean, centralized approach to game session management with comprehensive monitoring and flexible access control.
