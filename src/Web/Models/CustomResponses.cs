namespace GameServer.Web.Models;

public record InfoResponse(
    string Username, 
    string Email,
    bool IsEmailConfirmed);

public record TwoFactorResponse(
    string SharedKey, 
    int RecoveryCodesLeft, 
    string[]? RecoveryCodes, 
    bool IsTwoFactorEnabled, 
    bool IsMachineRemembered);

