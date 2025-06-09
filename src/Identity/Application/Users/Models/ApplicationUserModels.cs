namespace ServerGame.Application.Users.Models;

public record TwoFactorResponse(string SharedKey, int RecoveryCodesLeft, string[]? RecoveryCodes, bool IsTwoFactorEnabled, bool IsMachineRemembered);
public record InfoResponse(string Email, bool IsEmailConfirmed);
