using FluentValidation.Validators;
using ServerGame.Application.TodoItems.Commands.CreateAccount;

namespace ServerGame.Application.TodoItems.Commands.DeleteAccount;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty()
            .EmailAddress(EmailValidationMode.AspNetCoreCompatible)
            .WithMessage("Invalid email format.");
        RuleFor(v => v.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(20)
            .Matches("^[a-zA-Z0-9_]+$") // Alphanumeric and underscores only
            .WithMessage("Username must be alphanumeric and can include underscores.");
    }
}
