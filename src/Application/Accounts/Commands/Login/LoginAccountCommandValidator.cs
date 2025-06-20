using GameServer.Application.Accounts.Commands.Create;
using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Commands.Login;

public class LoginAccountCommandValidator : AbstractValidator<LoginAccountCommand>
{
    public LoginAccountCommandValidator(
        IUser user)
    {
        RuleFor(x => user.Id)
            .NotNull().WithMessage("User ID cannot be null.");
    }
}
