namespace ServerGame.Api.Models;

public record RegisterRequest(string Username, string Email, string Password);

public record LoginRequest(string EmailOrUsername, string Password, string? TwoFactorCode, string? TwoFactorRecoveryCode);

public record RefreshRequest(string RefreshToken);

public record ResendConfirmationEmailRequest(string Email);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string ResetCode, string NewPassword);

public record TwoFactorRequest(bool? Enable, string? TwoFactorCode, bool ResetSharedKey, bool ResetRecoveryCodes, bool ForgetMachine);

public record InfoRequest(string? NewEmail, string? NewPassword, string? OldPassword);
