namespace ServerGame.Application.Common.Interfaces.Persistence;

/// <summary>
/// Changes Tracking Type for DbSet operations
/// </summary>
public enum TrackingType
{
    NoTracking,
    NoTrackingWithIdentityResolution,
    Tracking
}
