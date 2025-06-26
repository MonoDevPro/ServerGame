using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Security;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Characters.Commands.Examples;

// Exemplo 1: Comando que requer personagem online e nível mínimo 10
[RequireCharacter(MinCharacterLevel = 10)]
public record EnterDungeonCommand() : IRequest<EnterDungeonResult>;

// Exemplo 2: Comando que permite personagem offline (útil para gerenciamento de inventário)
[RequireCharacter(AllowOfflineCharacter = true)]
public record ManageInventoryCommand() : IRequest<ManageInventoryResult>;

// Exemplo 3: Comando que permite execução sem personagem selecionado (listar personagens)
[RequireCharacter(AllowNoCharacterSelected = true)]
public record ListAccountCharactersCommand() : IRequest<ListCharactersResult>;

// Exemplo 4: Comando avançado com múltiplas restrições
[RequireCharacter(MinCharacterLevel = 50, AllowOfflineCharacter = false)]
public record EnterPvpArenaCommand() : IRequest<EnterPvpResult>;

public record EnterDungeonResult(bool Success, string? Message);
public record ManageInventoryResult(bool Success, string? Message);
public record ListCharactersResult(bool Success, string? Message);
public record EnterPvpResult(bool Success, string? Message);

// Handler de exemplo
public class EnterDungeonCommandHandler(
    //ICharacterService characterService,
    ILogger<EnterDungeonCommandHandler> logger)
    : IRequestHandler<EnterDungeonCommand, EnterDungeonResult>
{
    public Task<EnterDungeonResult> Handle(EnterDungeonCommand request, CancellationToken cancellationToken)
    {
        // O behavior já validou:
        // - Sessão de jogo válida
        // - Personagem selecionado
        // - Personagem online
        // - Nível mínimo 10
        
        logger.LogInformation("Player entered dungeon - all validations passed automatically");
        
        return new Task<EnterDungeonResult>(() => new EnterDungeonResult(true, "Successfully entered dungeon!"));
    }
}
