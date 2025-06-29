using GameServer.Domain.Enums;

namespace GameServer.Application.Common.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireGameSessionAttribute : Attribute
{
    public AccountType MinAccountType { get; set; } = 0;
    public bool AllowExpiredSession { get; set; } = false;
}
