using GameServer.Application.Accounts.Services;
using GameServer.Application.Accounts.Services.Current;
using GameServer.Application.Characters.Services;

namespace GameServer.Application.Characters.Commands.Create;

public class CreateCharacterCommandValidator : AbstractValidator<CreateCharacterCommand>
{
    private readonly ICurrentAccountService _currentAccountService;
    private readonly ICharacterQueryService _query;

    public CreateCharacterCommandValidator(ICharacterQueryService query, ICurrentAccountService currentAccountService)
    {
        _query = query ?? throw new ArgumentNullException(nameof(query));
        _currentAccountService = currentAccountService ?? throw new ArgumentNullException(nameof(currentAccountService));

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

        RuleFor(v => v.Name)
            .MustAsync(BeUniqueCharacterName)
            .WithMessage("Character name already exists.")
            .WithErrorCode("NameAlreadyExists");
    }

    private async Task<bool> BeAbleToCreateCharacter(CreateCharacterCommand command, CancellationToken cancellationToken)
    {
        var accountId = await _currentAccountService.GetIdAsync(cancellationToken);
        
        return await _query.CanCreateCharacterAsync(accountId, cancellationToken);
    }

    private async Task<bool> BeUniqueCharacterName(string name, CancellationToken cancellationToken)
    {
        return await _query.IsCharacterNameUniqueAsync(name, cancellationToken);
    }
}
