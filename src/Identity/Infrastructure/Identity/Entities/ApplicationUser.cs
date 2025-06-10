using MediatR;
using Microsoft.AspNetCore.Identity;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Interfaces.Events;
using ServerGame.Domain.Entities.Accounts;

namespace ServerGame.Infrastructure.Identity.Entities;

public class ApplicationUser : IdentityUser, IHasNotifications
{
    /// <summary>
    /// Referência para o agregado de domínio Account associado
    /// </summary>
    public long? AccountId { get; init; }
    
    /// <summary>
    /// Propriedade de navegação para o Account (optional)
    /// </summary>
    public Account? Account { get; init; }

    /// <summary>
    /// Eventos de domínio para este usuário
    /// </summary>
    private readonly List<INotification> _notifications = [];
    public IReadOnlyCollection<INotification> PendingNotifications => _notifications.AsReadOnly();

    /// <summary>
    /// Limpa os eventos de domínio
    /// </summary>
    public void ClearPendingNotifications()
    {
        _notifications.Clear();
    }

    /// <summary>
    /// Adiciona um evento de domínio
    /// </summary>
    public void AddNotification(INotification @event)
    {
        _notifications.Add(@event);
    }
}
