namespace GameServer.Application.Common.Security;

/// <summary>
/// Atributo que requer que o usuário tenha uma sessão de jogo válida E um personagem selecionado.
/// Herda automaticamente de RequireGameSessionAttribute, eliminando a necessidade de usar ambos.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireCharacterAttribute : RequireGameSessionAttribute
{
    public bool AllowNotOwner { get; set; } = false;

    public RequireCharacterAttribute()
    {
        // Por padrão, requer sessão válida (não expirada) quando se trata de personagens
        AllowExpiredSession = false;
    }
}
