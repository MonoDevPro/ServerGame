namespace ServerGame.Application.ApplicationUsers.Models;

public record TwoFactorResponse(string SharedKey, int RecoveryCodesLeft, string[]? RecoveryCodes, bool IsTwoFactorEnabled, bool IsMachineRemembered);
public record InfoResponse(string Username, string Email, bool IsEmailConfirmed);
