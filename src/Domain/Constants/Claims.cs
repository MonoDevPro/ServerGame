using System.Security.Claims;

namespace GameServer.Domain.Constants;

public static class Claims
{
    public const string AccountId = "account_id";
    public const string SessionStartTime = "session_start_time";
    public const string AccountLevel = "account_level";
    public const string LastActivity = "last_activity";
    public const string SelectedCharacterId = "selected_character_id";
}
