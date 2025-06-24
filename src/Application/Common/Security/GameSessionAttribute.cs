namespace GameServer.Application.Common.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireGameSessionAttribute : Attribute
{
    public bool AllowExpiredSession { get; set; } = false;
    public int MinimumLevel { get; set; } = 0;
}
