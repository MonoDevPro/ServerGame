using GameServer.Application.Accounts.Services;
using GameServer.Application.Accounts.Services.Current;
using GameServer.Application.Characters.Queries.Models;
using GameServer.Application.Characters.Services;
using GameServer.Application.Characters.Services.Current;
using GameServer.Application.Common.Interfaces;

namespace GameServer.Infrastructure.Services.Characters.Current;

/// <summary>
/// Lista os personagens (resumo) da conta do usuário autenticado.
/// </summary>
public class CurrentCharacterList(
    ICurrentAccountService currentAccount,
    ICharacterQueryService characterQuery)
    : ICurrentCharacterList
{
    public async Task<List<CharacterSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        // Obtém o ID da conta e lista personagens
        var accountId = await currentAccount.GetIdAsync(cancellationToken);
        return await characterQuery.GetAccountCharactersAsync(accountId, cancellationToken);
    }
}
