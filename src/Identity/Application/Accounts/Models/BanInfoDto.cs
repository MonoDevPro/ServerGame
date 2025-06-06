using ServerGame.Domain.Enums;

namespace ServerGame.Application.Accounts.Models;

public class BanInfoDto
{
    public BanStatus Status { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Reason { get; set; }
    public long? BannedById { get; set; }
}
