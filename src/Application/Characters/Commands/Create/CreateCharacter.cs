using GameServer.Application.Accounts.Services;
using GameServer.Application.Accounts.Services.Current;
using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Security;
using GameServer.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Characters.Commands.Create;

[RequireGameSession(AllowExpiredSession = false, MinAccountType = AccountType.Player)]
public record CreateCharacterCommand(
    string Name,
    CharacterClass Class
) : IRequest<CreateCharacterResult>;

public record CreateCharacterResult(
    bool Success,
    long? CharacterId,
    string? ErrorMessage
);

public class CreateCharacterCommandHandler(
    ICurrentAccountService account,
    ICharacterCommandService command,
    ILogger<CreateCharacterCommandHandler> logger
    ) : IRequestHandler<CreateCharacterCommand, CreateCharacterResult>
{
    public async Task<CreateCharacterResult> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
    {
        var accountId = await account.GetIdAsync(cancellationToken);
        
        // Cria o personagem
        var character = await command.CreateAsync(
            accountId,
            request.Name,
            request.Class,
            cancellationToken
        );

        logger.LogInformation("Character created successfully: {CharacterName} (ID: {CharacterId})",
            character.Name, character.Id);

        return new CreateCharacterResult(true, character.Id, null);
    }
}
