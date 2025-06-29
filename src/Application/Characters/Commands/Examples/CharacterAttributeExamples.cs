using GameServer.Application.Characters.Services;
using GameServer.Application.Common.Security;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Characters.Commands.Examples;

// Exemplo 1: Comando que requer personagem online e nível mínimo 10
[RequireCharacter]
public record EnterDungeonCommand() : IRequest<Unit>;

// Exemplo 2: Comando que permite personagem offline (útil para gerenciamento de inventário)
[RequireCharacter]
public record ManageInventoryCommand() : IRequest<Unit>;

// Exemplo 3: Comando que permite execução sem personagem selecionado (listar personagens)
[RequireCharacter]
public record ListAccountCharactersCommand() : IRequest<Unit>;

// Exemplo 4: Comando avançado com múltiplas restrições
[RequireCharacter]
public record EnterPvpArenaCommand() : IRequest<Unit>;

// Handler de exemplo
public class EnterDungeonCommandHandler(
    ILogger<EnterDungeonCommandHandler> logger
) : IRequestHandler<EnterDungeonCommand, Unit>, 
    IRequestHandler<ManageInventoryCommand, Unit>, 
    IRequestHandler<ListAccountCharactersCommand, Unit>, 
    IRequestHandler<EnterPvpArenaCommand, Unit>
{
    public Task<Unit> Handle(EnterDungeonCommand request, CancellationToken cancellationToken)
    {
        // O behavior já validou:
        // - Sessão de jogo válida
        // - Personagem selecionado
        // - Personagem online
        // - Nível mínimo 10
        
        logger.LogInformation("Player entered dungeon - all validations passed automatically");
        
        return Task.FromResult(Unit.Value);
    }
    
    public Task<Unit> Handle(ManageInventoryCommand request, CancellationToken cancellationToken)
    {
        // Permite gerenciamento de inventário mesmo offline
        logger.LogInformation("Player managing inventory - character can be offline");
        
        return Task.FromResult(Unit.Value);
    }
    
    public Task<Unit> Handle(ListAccountCharactersCommand request, CancellationToken cancellationToken)
    {
        // Permite listar personagens mesmo sem seleção
        logger.LogInformation("Listing account characters - no character selection required");
        
        return Task.FromResult(Unit.Value);
    }
    
    public Task<Unit> Handle(EnterPvpArenaCommand request, CancellationToken cancellationToken)
    {
        // Comando avançado com múltiplas restrições
        logger.LogInformation("Player entered PVP arena - all validations passed automatically");
        
        return Task.FromResult(Unit.Value);
    }
}
