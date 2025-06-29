using GameServer.Domain.Rules;

namespace GameServer.Application.Users.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .Must(UsernameRule.IsValid)
            .WithMessage("Username must be between 3-20 characters and contain only letters, numbers, and underscores.");

        RuleFor(v => v.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .Must(EmailRule.IsValid)
            .WithMessage("Invalid email format.");

        RuleFor(v => v.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");
    }
}
