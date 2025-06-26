using GameServer.Application.Characters.Services;

namespace GameServer.Application.Characters.Commands.Delete;

public class DeleteCharacterCommandValidator : AbstractValidator<DeleteCharacterCommand>
{
    private readonly ICurrentCharacterService _currentCharacterService;

    public DeleteCharacterCommandValidator(ICurrentCharacterService currentCharacterService)
    {
        _currentCharacterService = currentCharacterService ?? throw new ArgumentNullException(nameof(currentCharacterService));

        RuleFor(v => v.CharacterId)
            .GreaterThan(0)
            .WithMessage("Character ID must be greater than 0.");

        RuleFor(v => v)
            .MustAsync(BeCharacterOwner)
            .WithMessage("Character not found or you don't have permission to delete it.")
            .WithErrorCode("Forbidden");
    }

    private async Task<bool> BeCharacterOwner(DeleteCharacterCommand command, CancellationToken cancellationToken)
    {
        return await _currentCharacterService.IsCharacterOwnerAsync(command.CharacterId, cancellationToken);
    }
}
