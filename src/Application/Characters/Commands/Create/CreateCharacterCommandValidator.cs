using GameServer.Application.Characters.Services;
using GameServer.Domain.Enums;

namespace GameServer.Application.Characters.Commands.Create;

public class CreateCharacterCommandValidator : AbstractValidator<CreateCharacterCommand>
{
    private readonly ICurrentCharacterService _currentCharacterService;

    public CreateCharacterCommandValidator(ICurrentCharacterService currentCharacterService)
    {
        _currentCharacterService = currentCharacterService ?? throw new ArgumentNullException(nameof(currentCharacterService));

        RuleFor(v => v.Name)
            .NotEmpty()
            .WithMessage("Character name is required.")
            .Length(3, 20)
            .WithMessage("Character name must be between 3 and 20 characters.")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Character name can only contain letters, numbers, and underscores.");

        RuleFor(v => v.Class)
            .IsInEnum()
            .WithMessage("Invalid character class.");

        RuleFor(v => v)
            .MustAsync(BeAbleToCreateCharacter)
            .WithMessage("Account has reached the maximum number of characters (3).")
            .WithErrorCode("MaxCharactersReached");
    }

    private async Task<bool> BeAbleToCreateCharacter(CreateCharacterCommand command, CancellationToken cancellationToken)
    {
        return await _currentCharacterService.CanCreateCharacterAsync(cancellationToken);
    }
}
