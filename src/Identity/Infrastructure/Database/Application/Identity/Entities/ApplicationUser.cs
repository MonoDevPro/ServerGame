using MediatR;
using Microsoft.AspNetCore.Identity;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Users.Notifications;
using ServerGame.Domain.Entities.Accounts;

namespace ServerGame.Infrastructure.Database.Application.Identity.Entities;

public class ApplicationUser : IdentityUser, IHasNotifications
{
    /// <summary>
    /// Referência para o agregado de domínio Account associado
    /// </summary>
    public virtual long? AccountId { get; set; }
    
    /// <summary>
    /// Propriedade de navegação para o Account (optional)
    /// </summary>
    public virtual Account? Account { get; set; }

    /// <summary>
    /// Eventos de domínio para este usuário
    /// </summary>
    private readonly List<INotification> _notifications = [];
    public IReadOnlyCollection<INotification> PendingNotifications => _notifications.AsReadOnly();

    // Método de fábrica para criar e adicionar notificação
    public static ApplicationUser Create(string userName, string email)
    {
        var user = new ApplicationUser();
        user.AddNotification(new ApplicationUserCreatedNotification(
            user.Id,
            Guard.Against.Null(userName),
            Guard.Against.Null(email)
        ));
        return user;
    }

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
