namespace ServerGame.Application.Accounts.Models;

public record PurgeAccountsResult
{
    public int TotalAccountsFound { get; set; }
    public List<string> UserNames { get; set; } = [];
    public List<string> Errors { get; set; } = [];
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
