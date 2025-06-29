using GameServer.Application.Accounts.Queries.Models;

namespace GameServer.Application.Accounts.Services.Current;

/// <summary>
/// Serviço que gerencia a conta associada ao usuário atual autenticado.
/// </summary>
public interface ICurrentAccountService
{
    /// <summary>
    /// Retorna true se o usuário autenticado já possui uma Account no domínio de jogo.
    /// </summary>
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);
    
    Task<long> GetIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna o DTO da Account do usuário autenticado.
    /// </summary>
    Task<AccountDto> GetDtoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria (ou garante a existência de) uma Account para o usuário autenticado e retorna o DTO.
    /// </summary>
    Task<AccountDto> EnsureCreatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a Account do usuário autenticado.
    /// </summary>
    Task PurgeAsync(CancellationToken cancellationToken = default);
}
