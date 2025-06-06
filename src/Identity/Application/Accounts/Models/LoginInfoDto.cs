namespace ServerGame.Application.Accounts.Models;

public class LoginInfoDto
{
    public string LastLoginIp { get; set; } = default!;
    public DateTime LastLoginDate { get; set; } = default!;
}
