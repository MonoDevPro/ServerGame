namespace ServerGame.Application.Users.Commands;

public record RegisterCommand(string Username, string Email, string Password);

public record LoginCommand(string Username, string Password, string? TwoFactorCode, string? TwoFactorRecoveryCode);

public record RefreshCommand(string RefreshToken);

public record ResendConfirmationEmailCommand(string Email);

public record ForgotPasswordCommand(string Email);

public record ResetPasswordCommand(string Email, string ResetCode, string NewPassword);

public record TwoFactorCommand(bool? Enable, string? TwoFactorCode, bool ResetSharedKey, bool ResetRecoveryCodes, bool ForgetMachine);

public record InfoCommand(string? NewEmail, string? NewPassword, string? OldPassword);
