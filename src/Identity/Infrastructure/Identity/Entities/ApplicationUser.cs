using MediatR;
using Microsoft.AspNetCore.Identity;
using ServerGame.Application.Common.Interfaces;
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
    private readonly List<INotification> _events = [];
    public IReadOnlyCollection<INotification> Notifications => _events.AsReadOnly();

    /// <summary>
    /// Limpa os eventos de domínio
    /// </summary>
    public void ClearEvents()
    {
        _events.Clear();
    }

    /// <summary>
    /// Adiciona um evento de domínio
    /// </summary>
    public void AddEvent(INotification @event)
    {
        _events.Add(@event);
    }
}
