namespace GameServer.Application.Common.Security;

/// <summary>
/// Atributo que requer que o usuário tenha uma sessão de jogo válida E um personagem selecionado.
/// Herda automaticamente de RequireGameSessionAttribute, eliminando a necessidade de usar ambos.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireCharacterAttribute : RequireGameSessionAttribute
{
    /// <summary>
    /// Se verdadeiro, permite que o comando seja executado mesmo se o personagem estiver offline.
    /// Por padrão, requer que o personagem esteja online.
    /// </summary>
    public bool AllowOfflineCharacter { get; set; } = false;

    /// <summary>
    /// Nível mínimo do personagem necessário para executar o comando.
    /// 0 = sem restrição de nível.
    /// </summary>
    public int MinCharacterLevel { get; set; } = 0;

    /// <summary>
    /// Se verdadeiro, permite que o comando seja executado mesmo se não houver personagem selecionado,
    /// mas ainda requer sessão de jogo válida. Útil para comandos que podem criar ou listar personagens.
    /// </summary>
    public bool AllowNoCharacterSelected { get; set; } = false;

    public RequireCharacterAttribute()
    {
        // Por padrão, requer sessão válida (não expirada) quando se trata de personagens
        AllowExpiredSession = false;
    }
}
