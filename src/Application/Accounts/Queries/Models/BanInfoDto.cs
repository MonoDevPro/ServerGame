using GameServer.Domain.Enums;

namespace GameServer.Application.Accounts.Queries.Models;

public class BanInfoDto
{
    public BanStatus Status { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Reason { get; set; }
    public long? BannedById { get; set; }
}
